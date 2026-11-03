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
