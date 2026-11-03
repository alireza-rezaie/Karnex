using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reminders;

public record GetRemindersQuery(string? UserId, bool UpcomingOnly) : IRequest<IReadOnlyList<ReminderDto>>;
public record GetReminderByIdQuery(int Id) : IRequest<ReminderDto>;
