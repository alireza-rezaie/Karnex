using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects;

public record GetProjectsQuery : IRequest<IReadOnlyList<ProjectDto>>;
public record GetProjectByIdQuery(int Id) : IRequest<ProjectDetailsDto>;
public record CreateProjectCommand(string Key, string Name, string? Description, ProjectType Type, ProjectPrivacy Privacy, string? Icon, string? Color, DateTime? StartDate, DateTime? EndDate) : IRequest<ProjectDto>;
public record UpdateProjectCommand(int Id, string Name, string? Description, ProjectType Type, ProjectStatus Status, ProjectPrivacy Privacy, string? Icon, string? Color, DateTime? StartDate, DateTime? EndDate) : IRequest<ProjectDto>;
public record DeleteProjectCommand(int Id) : IRequest<bool>;
public record ArchiveProjectCommand(int Id) : IRequest<ProjectDto>;
public record AddProjectMemberCommand(int ProjectId, string UserId, string? Role) : IRequest<ProjectMemberDto>;
public record UpdateProjectMemberRoleCommand(int ProjectId, string UserId, string? Role) : IRequest<ProjectMemberDto>;
public record RemoveProjectMemberCommand(int ProjectId, string UserId) : IRequest<bool>;

public class ProjectHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>, IRequestHandler<GetProjectByIdQuery, ProjectDetailsDto>, IRequestHandler<CreateProjectCommand, ProjectDto>, IRequestHandler<UpdateProjectCommand, ProjectDto>, IRequestHandler<DeleteProjectCommand, bool>, IRequestHandler<ArchiveProjectCommand, ProjectDto>, IRequestHandler<AddProjectMemberCommand, ProjectMemberDto>, IRequestHandler<UpdateProjectMemberRoleCommand, ProjectMemberDto>, IRequestHandler<RemoveProjectMemberCommand, bool>
{
    private DbSet<Project> Projects => unitOfWork.dbContext.Set<Project>();
    private DbSet<ProjectMember> ProjectMembers => unitOfWork.dbContext.Set<ProjectMember>();
    public async Task<IReadOnlyList<ProjectDto>> Handle(GetProjectsQuery r, CancellationToken ct) => (await Projects.AsNoTracking().Include(x => x.WorkItems).OrderBy(x => x.Name).ToListAsync(ct)).Select(ToDto).ToList();
    public async Task<ProjectDetailsDto> Handle(GetProjectByIdQuery r, CancellationToken ct)
    {
        var project = await Projects.AsNoTracking().Include(x => x.WorkItems).Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException();
        return ToDetailsDto(project);
    }
    public async Task<ProjectDto> Handle(CreateProjectCommand r, CancellationToken ct)
    {
        var key = r.Key.Trim().ToUpperInvariant();
        if (await Projects.AnyAsync(x => x.Key == key, ct)) throw new BadRequestEntityException("پروژه‌ای با این کلید وجود دارد.");
        var project = new Project { Key = key, Name = r.Name.Trim(), Description = r.Description?.Trim() ?? string.Empty, Type = r.Type, Privacy = r.Privacy, Icon = r.Icon ?? "folder", Color = r.Color ?? "#3b82f6", StartDate = r.StartDate, EndDate = r.EndDate };
        await unitOfWork.Repository<Project>().AddAsync(project, ct); await unitOfWork.Save(ct); return ToDto(project);
    }
    public async Task<ProjectDto> Handle(UpdateProjectCommand r, CancellationToken ct)
    {
        var project = await Projects.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException();
        project.Name = r.Name.Trim(); project.Description = r.Description?.Trim() ?? string.Empty; project.Type = r.Type; project.Status = r.Status; project.Privacy = r.Privacy; project.Icon = r.Icon ?? "folder"; project.Color = r.Color ?? "#3b82f6"; project.StartDate = r.StartDate; project.EndDate = r.EndDate; project.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct); return ToDto(project);
    }
    public async Task<bool> Handle(DeleteProjectCommand r, CancellationToken ct) { var project = await Projects.FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); project.IsDelete = true; project.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return true; }
    public async Task<ProjectDto> Handle(ArchiveProjectCommand r, CancellationToken ct)
    {
        var project = await Projects.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException();
        project.Status = ProjectStatus.Archived;
        project.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct);
        return ToDto(project);
    }
    public async Task<ProjectMemberDto> Handle(AddProjectMemberCommand r, CancellationToken ct)
    {
        if (!await Projects.AnyAsync(x => x.Id == r.ProjectId, ct)) throw new NotFoundEntityException();
        var userId = RequiredUserId(r.UserId);
        if (await ProjectMembers.AnyAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct)) throw new BadRequestEntityException("این کاربر از قبل عضو پروژه است.");
        var member = new ProjectMember { ProjectId = r.ProjectId, UserId = userId, Role = NormalizeRole(r.Role) };
        await ProjectMembers.AddAsync(member, ct);
        await unitOfWork.Save(ct);
        return ToMemberDto(member);
    }
    public async Task<ProjectMemberDto> Handle(UpdateProjectMemberRoleCommand r, CancellationToken ct)
    {
        var userId = RequiredUserId(r.UserId);
        var member = await ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct) ?? throw new NotFoundEntityException();
        member.Role = NormalizeRole(r.Role);
        await unitOfWork.Save(ct);
        return ToMemberDto(member);
    }
    public async Task<bool> Handle(RemoveProjectMemberCommand r, CancellationToken ct)
    {
        var userId = RequiredUserId(r.UserId);
        var member = await ProjectMembers.FirstOrDefaultAsync(x => x.ProjectId == r.ProjectId && x.UserId == userId, ct) ?? throw new NotFoundEntityException();
        ProjectMembers.Remove(member);
        await unitOfWork.Save(ct);
        return true;
    }
    private static ProjectDto ToDto(Project x) { var total = x.WorkItems.Count; var completed = x.WorkItems.Count(w => w.Status == WorkItemStatus.Done); return new(x.Id, x.Key, x.Name, x.Description, x.Type.ToString().ToLowerInvariant(), x.Status.ToString().ToLowerInvariant(), x.Privacy.ToString().ToLowerInvariant(), x.Icon, x.Color, total == 0 ? 0 : (int)Math.Round(completed * 100d / total), x.StartDate, x.EndDate, total, completed); }
    private static ProjectDetailsDto ToDetailsDto(Project x)
    {
        var project = ToDto(x);
        return new(project.Id, project.Key, project.Name, project.Description, project.Type, project.Status, project.Privacy, project.Icon, project.Color, project.Progress, project.StartDate, project.EndDate, project.TasksCount, project.CompletedTasks, x.Members.OrderBy(member => member.UserId).Select(ToMemberDto).ToList());
    }
    private static ProjectMemberDto ToMemberDto(ProjectMember member) => new(member.UserId, NormalizeRole(member.Role));
    private static string RequiredUserId(string value) => string.IsNullOrWhiteSpace(value) ? throw new BadRequestEntityException("شناسهٔ کاربر الزامی است.") : value.Trim();
    private static string NormalizeRole(string? value)
    {
        var role = value?.Trim();
        if (string.IsNullOrWhiteSpace(role)) return "member";
        return role.ToLowerInvariant() switch
        {
            "project manager" or "project-manager" or "project_manager" => "pm",
            "scrum master" or "scrum-master" or "scrum_master" => "sm",
            _ => role,
        };
    }
}
