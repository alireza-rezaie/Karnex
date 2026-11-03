using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reminders;

public record CreateReminderCommand(string Title, string? Description, DateTime ReminderAt, WorkItemPriority Priority, string? UserId, int? ProjectId, int? WorkItemId) : IRequest<ReminderDto>;
public record UpdateReminderCommand(int Id, string Title, string? Description, DateTime ReminderAt, WorkItemPriority Priority, string? UserId, int? ProjectId, int? WorkItemId, bool IsCompleted) : IRequest<ReminderDto>;
public record CompleteReminderCommand(int Id, bool IsCompleted) : IRequest<ReminderDto>;
public record DeleteReminderCommand(int Id) : IRequest<bool>;
