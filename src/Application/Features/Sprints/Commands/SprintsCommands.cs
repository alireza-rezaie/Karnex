using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sprints;

public record CreateSprintCommand(int ProjectId, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;
public record UpdateSprintCommand(int Id, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;
public record ChangeSprintStatusCommand(int Id, SprintStatus Status) : IRequest<SprintDto>;
public record DeleteSprintCommand(int Id) : IRequest<bool>;
