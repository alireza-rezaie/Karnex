using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.WorkItems;

public record CreateWorkItemCommand(int ProjectId, string Title, string? Description, WorkItemType Type, WorkItemPriority Priority, string? AssigneeId, string? ReporterId, int? SprintId, int? ParentId, int? StoryPoints, DateTime? DueDate, List<LabelDto>? Labels) : IRequest<WorkItemDto>;
public record UpdateWorkItemCommand(int Id, string Title, string? Description, WorkItemType Type, WorkItemPriority Priority, string? AssigneeId, int? SprintId, int? ParentId, int? StoryPoints, DateTime? DueDate, List<LabelDto>? Labels) : IRequest<WorkItemDto>;
public record ChangeWorkItemStatusCommand(int Id, WorkItemStatus Status) : IRequest<WorkItemDto>;
public record DeleteWorkItemCommand(int Id) : IRequest<bool>;
public record AddWorkItemCommentCommand(int Id, string AuthorId, string Content) : IRequest<bool>;
public record AddWorkItemAttachmentCommand(int Id, string FileName, string FileUrl, string? ContentType, long Size) : IRequest<bool>;
