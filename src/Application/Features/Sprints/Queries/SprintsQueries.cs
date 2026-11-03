using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sprints;

public record GetSprintsQuery(int? ProjectId) : IRequest<IReadOnlyList<SprintDto>>;
