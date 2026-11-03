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
