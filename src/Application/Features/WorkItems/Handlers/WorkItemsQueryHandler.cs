using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.WorkItems;

public sealed class WorkItemQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetWorkItemsQuery, IReadOnlyList<WorkItemDto>>, IRequestHandler<GetMyWorkItemsQuery, IReadOnlyList<WorkItemDto>>, IRequestHandler<GetWorkItemByIdQuery, WorkItemDto>, IRequestHandler<GetDashboardTaskStatsQuery, DashboardTaskStatsDto>
{
private DbSet<WorkItem> Items => unitOfWork.dbContext.Set<WorkItem>();
public async Task<IReadOnlyList<WorkItemDto>> Handle(GetWorkItemsQuery r, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        var query = Items.AsNoTracking().Where(x => userId != null && unitOfWork.dbContext.Set<ProjectMember>()
            .Any(member => member.ProjectId == x.ProjectId && member.UserId == userId)).AsQueryable();
        if (r.ProjectId.HasValue) query = query.Where(x => x.ProjectId == r.ProjectId);
        if (r.SprintId.HasValue) query = query.Where(x => x.SprintId == r.SprintId);
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        if (r.Priority.HasValue) query = query.Where(x => x.Priority == r.Priority);
        if (!string.IsNullOrWhiteSpace(r.AssigneeId)) query = query.Where(x => x.AssigneeId == r.AssigneeId);
        if (!string.IsNullOrWhiteSpace(r.Search)) query = query.Where(x => x.Title.Contains(r.Search) || x.Key.Contains(r.Search));
        return (await Detail(query).OrderByDescending(x => x.Created).ToListAsync(ct)).Select(ToDto).ToList();
    }

public async Task<IReadOnlyList<WorkItemDto>> Handle(GetMyWorkItemsQuery r, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        return (await Detail(Items.AsNoTracking().Where(x => x.AssigneeId == userId && unitOfWork.dbContext.Set<ProjectMember>()
            .Any(member => member.ProjectId == x.ProjectId && member.UserId == userId))).OrderByDescending(x => x.Created).ToListAsync(ct)).Select(ToDto).ToList();
    }

public async Task<WorkItemDto> Handle(GetWorkItemByIdQuery r, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        var item = await Detail(Items.AsNoTracking().Where(x => x.Id == r.Id && userId != null &&
            unitOfWork.dbContext.Set<ProjectMember>().Any(member => member.ProjectId == x.ProjectId && member.UserId == userId)))
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundEntityException();
        return ToDto(item);
    }

public async Task<DashboardTaskStatsDto> Handle(GetDashboardTaskStatsQuery r, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        var query = Items.AsNoTracking().Where(x => userId != null &&
            unitOfWork.dbContext.Set<ProjectMember>().Any(member => member.ProjectId == x.ProjectId && member.UserId == userId) &&
            (!r.ProjectId.HasValue || x.ProjectId == r.ProjectId)); var now = DateTime.UtcNow;
        return new(await query.CountAsync(ct), await query.CountAsync(x => x.Status == WorkItemStatus.InProgress, ct), await query.CountAsync(x => x.Status == WorkItemStatus.Done, ct), await query.CountAsync(x => x.DueDate < now && x.Status != WorkItemStatus.Done, ct));
    }

private IQueryable<WorkItem> AccessibleItems(IQueryable<WorkItem> query) { var userId = currentUserService.UserId; return query.Where(x => userId != null && unitOfWork.dbContext.Set<ProjectMember>().Any(member => member.ProjectId == x.ProjectId && member.UserId == userId)); }
    private static IQueryable<WorkItem> Detail(IQueryable<WorkItem> query) => query.Include(x => x.Labels).Include(x => x.Comments).Include(x => x.Attachments).Include(x => x.Subtasks);
    private static WorkItemDto ToDto(WorkItem x) => new(x.Id, x.Key, x.Title, x.Description, x.Type.ToString().ToLowerInvariant(), x.Status.ToString().ToLowerInvariant(), x.Priority.ToString().ToLowerInvariant(), x.AssigneeId, x.ReporterId, x.ProjectId, x.SprintId, x.ParentId, x.Labels.Select(l => l.Name).ToList(), x.StoryPoints, x.DueDate, x.Created, x.lastModified, x.Comments.Count, x.Attachments.Count, x.Subtasks.Count, x.Subtasks.Count(s => s.Status == WorkItemStatus.Done));
}
