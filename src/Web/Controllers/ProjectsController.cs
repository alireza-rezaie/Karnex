using Application.Dtos.ProjectManagement;
using Application.Features.Projects;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

public class ProjectsController : BaseApiController
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetProjectsQuery(), cancellationToken));
    [HttpGet("{id:int}")] public async Task<ActionResult<ProjectDetailsDto>> GetById(int id, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetProjectByIdQuery(id), cancellationToken));
    [HttpPost] public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectCommand request, CancellationToken cancellationToken) { var project = await Mediator.Send(request, cancellationToken); return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project); }
    [HttpPut("{id:int}")] public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpdateProjectCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
    [HttpPatch("{id:int}/archive")] public async Task<ActionResult<ProjectDto>> Archive(int id, CancellationToken cancellationToken) => Ok(await Mediator.Send(new ArchiveProjectCommand(id), cancellationToken));
    [HttpPost("{id:int}/members")] public async Task<ActionResult<ProjectMemberDto>> AddMember(int id, [FromBody] AddProjectMemberCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { ProjectId = id }, cancellationToken));
    [HttpPut("{id:int}/members/{userId}")] public async Task<ActionResult<ProjectMemberDto>> UpdateMemberRole(int id, string userId, [FromBody] UpdateProjectMemberRoleCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { ProjectId = id, UserId = userId }, cancellationToken));
    [HttpDelete("{id:int}/members/{userId}")] public async Task<IActionResult> RemoveMember(int id, string userId, CancellationToken cancellationToken) { await Mediator.Send(new RemoveProjectMemberCommand(id, userId), cancellationToken); return NoContent(); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { await Mediator.Send(new DeleteProjectCommand(id), cancellationToken); return NoContent(); }
}
