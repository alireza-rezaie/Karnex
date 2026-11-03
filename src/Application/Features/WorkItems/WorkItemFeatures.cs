using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.WorkItems;

public record GetWorkItemsQuery(int? ProjectId, int? SprintId, WorkItemStatus? Status, WorkItemPriority? Priority, string? AssigneeId, string? Search) : IRequest<IReadOnlyList<WorkItemDto>>;
public record GetMyWorkItemsQuery : IRequest<IReadOnlyList<WorkItemDto>>;
public record GetWorkItemByIdQuery(int Id) : IRequest<WorkItemDto>;
public record GetDashboardTaskStatsQuery(int? ProjectId) : IRequest<DashboardTaskStatsDto>;
public record CreateWorkItemCommand(int ProjectId, string Title, string? Description, WorkItemType Type, WorkItemPriority Priority, string? AssigneeId, string? ReporterId, int? SprintId, int? ParentId, int? StoryPoints, DateTime? DueDate, List<LabelDto>? Labels) : IRequest<WorkItemDto>;
public record UpdateWorkItemCommand(int Id, string Title, string? Description, WorkItemType Type, WorkItemPriority Priority, string? AssigneeId, int? SprintId, int? ParentId, int? StoryPoints, DateTime? DueDate, List<LabelDto>? Labels) : IRequest<WorkItemDto>;
public record ChangeWorkItemStatusCommand(int Id, WorkItemStatus Status) : IRequest<WorkItemDto>;
public record DeleteWorkItemCommand(int Id) : IRequest<bool>;
public record AddWorkItemCommentCommand(int Id, string AuthorId, string Content) : IRequest<bool>;
public record AddWorkItemAttachmentCommand(int Id, string FileName, string FileUrl, string? ContentType, long Size) : IRequest<bool>;

public class WorkItemHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetWorkItemsQuery, IReadOnlyList<WorkItemDto>>, IRequestHandler<GetMyWorkItemsQuery, IReadOnlyList<WorkItemDto>>, IRequestHandler<GetWorkItemByIdQuery, WorkItemDto>, IRequestHandler<GetDashboardTaskStatsQuery, DashboardTaskStatsDto>, IRequestHandler<CreateWorkItemCommand, WorkItemDto>, IRequestHandler<UpdateWorkItemCommand, WorkItemDto>, IRequestHandler<ChangeWorkItemStatusCommand, WorkItemDto>, IRequestHandler<DeleteWorkItemCommand, bool>, IRequestHandler<AddWorkItemCommentCommand, bool>, IRequestHandler<AddWorkItemAttachmentCommand, bool>
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
    public async Task<WorkItemDto> Handle(CreateWorkItemCommand r, CancellationToken ct)
    {
        var userId = currentUserService.UserId;
        var project = await unitOfWork.dbContext.Set<Project>().FirstOrDefaultAsync(x => x.Id == r.ProjectId &&
            userId != null && unitOfWork.dbContext.Set<ProjectMember>().Any(member => member.ProjectId == x.Id && member.UserId == userId), ct)
            ?? throw new NotFoundEntityException();
        if (r.SprintId.HasValue && !await unitOfWork.dbContext.Set<Sprint>().AnyAsync(x => x.Id == r.SprintId && x.ProjectId == r.ProjectId, ct)) throw new BadRequestEntityException("اسپرینت برای این پروژه معتبر نیست.");
        if (r.ParentId.HasValue && !await Items.AnyAsync(x => x.Id == r.ParentId && x.ProjectId == r.ProjectId, ct)) throw new BadRequestEntityException("تسک والد برای این پروژه معتبر نیست.");
        var number = await Items.IgnoreQueryFilters().CountAsync(x => x.ProjectId == r.ProjectId, ct) + 1;
        var item = new WorkItem { ProjectId = r.ProjectId, Key = $"{project.Key}-{number}", Title = r.Title.Trim(), Description = r.Description?.Trim() ?? string.Empty, Type = r.Type, Priority = r.Priority, AssigneeId = r.AssigneeId, ReporterId = userId, SprintId = r.SprintId, ParentId = r.ParentId, StoryPoints = r.StoryPoints, DueDate = r.DueDate, CreatedBy = r.ReporterId ?? string.Empty, Labels = Labels(r.Labels) };
        await unitOfWork.Repository<WorkItem>().AddAsync(item, ct); await unitOfWork.Save(ct); return ToDto(item);
    }
    public async Task<WorkItemDto> Handle(UpdateWorkItemCommand r, CancellationToken ct)
    {
        var item = await Detail(AccessibleItems(Items)).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException();
        if (r.SprintId.HasValue && !await unitOfWork.dbContext.Set<Sprint>().AnyAsync(x => x.Id == r.SprintId && x.ProjectId == item.ProjectId, ct)) throw new BadRequestEntityException("اسپرینت نامعتبر است.");
        if (r.ParentId == r.Id) throw new BadRequestEntityException("تسک نمی‌تواند والد خودش باشد.");
        item.Title = r.Title.Trim(); item.Description = r.Description?.Trim() ?? string.Empty; item.Type = r.Type; item.Priority = r.Priority; item.AssigneeId = r.AssigneeId; item.SprintId = r.SprintId; item.ParentId = r.ParentId; item.StoryPoints = r.StoryPoints; item.DueDate = r.DueDate; item.lastModified = DateTime.UtcNow;
        unitOfWork.dbContext.Set<WorkItemLabel>().RemoveRange(item.Labels); item.Labels = Labels(r.Labels); await unitOfWork.Save(ct); return ToDto(item);
    }
    public async Task<WorkItemDto> Handle(ChangeWorkItemStatusCommand r, CancellationToken ct)
    {
        var item = await Detail(AccessibleItems(Items)).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); item.Status = r.Status; item.CompletedAt = r.Status == WorkItemStatus.Done ? DateTime.UtcNow : null; item.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(item);
    }
    public async Task<bool> Handle(DeleteWorkItemCommand r, CancellationToken ct) { var item = await AccessibleItems(Items).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); item.IsDelete = true; item.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return true; }
    public async Task<bool> Handle(AddWorkItemCommentCommand r, CancellationToken ct) { var userId = currentUserService.UserId; if (userId == null || !await AccessibleItems(Items).AnyAsync(x => x.Id == r.Id, ct)) throw new NotFoundEntityException(); await unitOfWork.Repository<WorkItemComment>().AddAsync(new() { WorkItemId = r.Id, AuthorId = userId, Content = r.Content.Trim(), CreatedBy = userId }, ct); await unitOfWork.Save(ct); return true; }
    public async Task<bool> Handle(AddWorkItemAttachmentCommand r, CancellationToken ct) { if (!await AccessibleItems(Items).AnyAsync(x => x.Id == r.Id, ct)) throw new NotFoundEntityException(); await unitOfWork.Repository<WorkItemAttachment>().AddAsync(new() { WorkItemId = r.Id, FileName = r.FileName.Trim(), FileUrl = r.FileUrl.Trim(), ContentType = r.ContentType, Size = r.Size }, ct); await unitOfWork.Save(ct); return true; }
    private IQueryable<WorkItem> AccessibleItems(IQueryable<WorkItem> query) { var userId = currentUserService.UserId; return query.Where(x => userId != null && unitOfWork.dbContext.Set<ProjectMember>().Any(member => member.ProjectId == x.ProjectId && member.UserId == userId)); }
    private static IQueryable<WorkItem> Detail(IQueryable<WorkItem> query) => query.Include(x => x.Labels).Include(x => x.Comments).Include(x => x.Attachments).Include(x => x.Subtasks);
    private static List<WorkItemLabel> Labels(IEnumerable<LabelDto>? labels) => labels?.Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x => new WorkItemLabel { Name = x.Name.Trim(), Color = string.IsNullOrWhiteSpace(x.Color) ? "#6b7280" : x.Color! }).ToList() ?? [];
    private static WorkItemDto ToDto(WorkItem x) => new(x.Id, x.Key, x.Title, x.Description, x.Type.ToString().ToLowerInvariant(), x.Status.ToString().ToLowerInvariant(), x.Priority.ToString().ToLowerInvariant(), x.AssigneeId, x.ReporterId, x.ProjectId, x.SprintId, x.ParentId, x.Labels.Select(l => l.Name).ToList(), x.StoryPoints, x.DueDate, x.Created, x.lastModified, x.Comments.Count, x.Attachments.Count, x.Subtasks.Count, x.Subtasks.Count(s => s.Status == WorkItemStatus.Done));
}
