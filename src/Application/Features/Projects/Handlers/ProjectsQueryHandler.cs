using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects;

public sealed class ProjectQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>, IRequestHandler<GetProjectByIdQuery, ProjectDetailsDto>
{
private DbSet<Project> Projects => unitOfWork.dbContext.Set<Project>();
public async Task<IReadOnlyList<ProjectDto>> Handle(GetProjectsQuery r, CancellationToken ct)
    {
        var userId = RequireCurrentUser();
        return (await Projects.AsNoTracking().Where(x => x.Members.Any(m => m.UserId == userId))
            .Include(x => x.WorkItems).OrderBy(x => x.Name).ToListAsync(ct)).Select(ToDto).ToList();
    }

public async Task<ProjectDetailsDto> Handle(GetProjectByIdQuery r, CancellationToken ct)
    {
        var userId = RequireCurrentUser();
        var project = await Projects.AsNoTracking().Where(x => x.Id == r.Id && x.Members.Any(m => m.UserId == userId))
            .Include(x => x.WorkItems).Include(x => x.Members).FirstOrDefaultAsync(ct) ?? throw new NotFoundEntityException();
        return ToDetailsDto(project);
    }

private string RequireCurrentUser() => !string.IsNullOrWhiteSpace(currentUser.UserId) ? currentUser.UserId : throw new BadRequestEntityException("ورود کاربر الزامی است.");
    private static ProjectDto ToDto(Project x) { var total = x.WorkItems.Count; var completed = x.WorkItems.Count(w => w.Status == WorkItemStatus.Done); return new(x.Id, x.Key, x.Name, x.Description, x.Type.ToString().ToLowerInvariant(), x.Status.ToString().ToLowerInvariant(), x.Privacy.ToString().ToLowerInvariant(), x.Icon, x.Color, total == 0 ? 0 : (int)Math.Round(completed * 100d / total), x.StartDate, x.EndDate, total, completed); }
    private static ProjectDetailsDto ToDetailsDto(Project x) { var p = ToDto(x); return new(p.Id, p.Key, p.Name, p.Description, p.Type, p.Status, p.Privacy, p.Icon, p.Color, p.Progress, p.StartDate, p.EndDate, p.TasksCount, p.CompletedTasks, x.Members.OrderBy(m => m.UserId).Select(ToMemberDto).ToList()); }
    private static ProjectMemberDto ToMemberDto(ProjectMember member) => new(member.UserId, NormalizeRole(member.Role));
    private static string NormalizeRole(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        null or "" => "member",
        "project manager" or "project-manager" or "project_manager" => "pm",
        "scrum master" or "scrum-master" or "scrum_master" => "sm",
        var role => role,
    };
}
