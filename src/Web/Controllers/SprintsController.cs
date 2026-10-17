using Application.Dtos.ProjectManagement;
using Application.Features.Sprints;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

public class SprintsController : BaseApiController
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<SprintDto>>> GetAll([FromQuery] int? projectId, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetSprintsQuery(projectId), cancellationToken));
    [HttpPost] public async Task<ActionResult<SprintDto>> Create([FromBody] CreateSprintCommand request, CancellationToken cancellationToken) { var sprint = await Mediator.Send(request, cancellationToken); return CreatedAtAction(nameof(GetAll), new { id = sprint.Id }, sprint); }
    [HttpPut("{id:int}")] public async Task<ActionResult<SprintDto>> Update(int id, [FromBody] UpdateSprintCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
    [HttpPatch("{id:int}/status")] public async Task<ActionResult<SprintDto>> ChangeStatus(int id, [FromBody] ChangeSprintStatusCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { await Mediator.Send(new DeleteSprintCommand(id), cancellationToken); return NoContent(); }
}
