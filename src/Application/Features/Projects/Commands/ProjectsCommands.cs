using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects;

public record CreateProjectCommand(string Key, string Name, string? Description, ProjectType Type, ProjectPrivacy Privacy, string? Icon, string? Color, DateTime? StartDate, DateTime? EndDate) : IRequest<ProjectDto>;
public record UpdateProjectCommand(int Id, string Name, string? Description, ProjectType Type, ProjectStatus Status, ProjectPrivacy Privacy, string? Icon, string? Color, DateTime? StartDate, DateTime? EndDate) : IRequest<ProjectDto>;
public record DeleteProjectCommand(int Id) : IRequest<bool>;
public record ArchiveProjectCommand(int Id) : IRequest<ProjectDto>;
public record AddProjectMemberCommand(int ProjectId, string UserId, string? Role) : IRequest<ProjectMemberDto>;
public record UpdateProjectMemberRoleCommand(int ProjectId, string UserId, string? Role) : IRequest<ProjectMemberDto>;
public record RemoveProjectMemberCommand(int ProjectId, string UserId) : IRequest<bool>;
