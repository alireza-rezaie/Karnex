using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects;

public sealed class ProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<CreateProjectCommand, ProjectDto>, IRequestHandler<UpdateProjectCommand, ProjectDto>, IRequestHandler<DeleteProjectCommand, bool>, IRequestHandler<ArchiveProjectCommand, ProjectDto>, IRequestHandler<AddProjectMemberCommand, ProjectMemberDto>, IRequestHandler<UpdateProjectMemberRoleCommand, ProjectMemberDto>, IRequestHandler<RemoveProjectMemberCommand, bool>
{
private DbSet<Project> Projects => unitOfWork.dbContext.Set<Project>();
    private DbSet<ProjectMember> ProjectMembers => unitOfWork.dbContext.Set<ProjectMember>();
public async Task<ProjectDto> Handle(CreateProjectCommand r, CancellationToken ct)
    {
        var userId = RequireCurrentUser();
        var key = r.Key.Trim().ToUpperInvariant();
        if (await Projects.AnyAsync(x => x.Key == key, ct)) throw new BadRequestEntityException("پروژه‌ای با این کلید وجود دارد.");
        var project = new Project { Key = key, Name = r.Name.Trim(), Description = r.Description?.Trim() ?? string.Empty, Type = r.Type, Privacy = r.Privacy, Icon = r.Icon ?? "folder", Color = r.Color ?? "#3b82f6", StartDate = r.StartDate, EndDate = r.EndDate };
        project.Members.Add(new ProjectMember { UserId = userId, Role = "pm" });
        await unitOfWork.Repository<Project>().AddAsync(project, ct);
        await unitOfWork.Save(ct);
        return ToDto(project);
    }

public async Task<ProjectDto> Handle(UpdateProjectCommand r, CancellationToken ct)
    {
        var project = await AccessibleManagedProject(r.Id, ct);
        project.Name = r.Name.Trim(); project.Description = r.Description?.Trim() ?? string.Empty; project.Type = r.Type; project.Status = r.Status; project.Privacy = r.Privacy; project.Icon = r.Icon ?? "folder"; project.Color = r.Color ?? "#3b82f6"; project.StartDate = r.StartDate; project.EndDate = r.EndDate; project.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct); return ToDto(project);
    }

public async Task<bool> Handle(DeleteProjectCommand r, CancellationToken ct)
    {
        var project = await AccessibleManagedProject(r.Id, ct);
        project.IsDelete = true; project.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return true;
    }

public async Task<ProjectDto> Handle(ArchiveProjectCommand r, CancellationToken ct)
    {
        var project = await AccessibleManagedProject(r.Id, ct);
        project.Status = ProjectStatus.Archived; project.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct); return ToDto(project);
    }

public async Task<ProjectMemberDto> Handle(AddProjectMemberCommand r, CancellationToken ct)
    {
        var project = await AccessibleManagedProject(r.ProjectId, ct);
        var userId = RequiredUserId(r.UserId);
        if (await ProjectMembers.AnyAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct)) throw new BadRequestEntityException("این کاربر از قبل عضو پروژه است.");
        var member = new ProjectMember { ProjectId = project.Id, UserId = userId, Role = NormalizeRole(r.Role) };
        await ProjectMembers.AddAsync(member, ct); await unitOfWork.Save(ct); return ToMemberDto(member);
    }

public async Task<ProjectMemberDto> Handle(UpdateProjectMemberRoleCommand r, CancellationToken ct)
    {
        await AccessibleManagedProject(r.ProjectId, ct);
        var userId = RequiredUserId(r.UserId);
        var member = await ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct) ?? throw new NotFoundEntityException();
        member.Role = NormalizeRole(r.Role); await unitOfWork.Save(ct); return ToMemberDto(member);
    }

public async Task<bool> Handle(RemoveProjectMemberCommand r, CancellationToken ct)
    {
        var project = await AccessibleManagedProject(r.ProjectId, ct);
        var userId = RequiredUserId(r.UserId);
        if (project.Members.Count == 1 && project.Members.Any(x => x.UserId == userId))
            throw new BadRequestEntityException("مالک پروژه نمی‌تواند تنها عضو پروژه را حذف کند.");
        var member = await ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct) ?? throw new NotFoundEntityException();
        ProjectMembers.Remove(member); await unitOfWork.Save(ct); return true;
    }

private async Task<Project> AccessibleManagedProject(int projectId, CancellationToken ct)
    {
        var userId = RequireCurrentUser();
        var project = await Projects.Include(x => x.WorkItems).Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == projectId && x.Members.Any(m => m.UserId == userId), ct)
            ?? throw new NotFoundEntityException();
        var role = project.Members.First(m => m.UserId == userId).Role;
        if (role is not ("pm" or "sm" or "PM" or "SM"))
            throw new BadRequestEntityException("فقط مدیر پروژه یا اسکرام‌مستر می‌تواند این عملیات را انجام دهد.");
        return project;
    }

    private string RequireCurrentUser() => !string.IsNullOrWhiteSpace(currentUser.UserId) ? currentUser.UserId : throw new BadRequestEntityException("ورود کاربر الزامی است.");
    private static ProjectDto ToDto(Project x) { var total = x.WorkItems.Count; var completed = x.WorkItems.Count(w => w.Status == WorkItemStatus.Done); return new(x.Id, x.Key, x.Name, x.Description, x.Type.ToString().ToLowerInvariant(), x.Status.ToString().ToLowerInvariant(), x.Privacy.ToString().ToLowerInvariant(), x.Icon, x.Color, total == 0 ? 0 : (int)Math.Round(completed * 100d / total), x.StartDate, x.EndDate, total, completed); }
    private static ProjectDetailsDto ToDetailsDto(Project x) { var p = ToDto(x); return new(p.Id, p.Key, p.Name, p.Description, p.Type, p.Status, p.Privacy, p.Icon, p.Color, p.Progress, p.StartDate, p.EndDate, p.TasksCount, p.CompletedTasks, x.Members.OrderBy(m => m.UserId).Select(ToMemberDto).ToList()); }
    private static ProjectMemberDto ToMemberDto(ProjectMember member) => new(member.UserId, NormalizeRole(member.Role));
    private static string RequiredUserId(string value) => string.IsNullOrWhiteSpace(value) ? throw new BadRequestEntityException("شناسهٔ کاربر الزامی است.") : value.Trim();
    private static string NormalizeRole(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        null or "" => "member",
        "project manager" or "project-manager" or "project_manager" => "pm",
        "scrum master" or "scrum-master" or "scrum_master" => "sm",
        var role => role,
    };
}
