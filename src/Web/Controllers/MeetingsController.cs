using Application.Dtos.ProjectManagement;
using Application.Features.Meetings;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Route("api/meetings")]
public class MeetingsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MeetingDto>>> GetAll([FromQuery] DateTime? date, [FromQuery] int? projectId, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMeetingsQuery(date, projectId), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MeetingDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMeetingByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<MeetingDto>> Create(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await Mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = meeting.Id }, meeting);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MeetingDto>> Update(int id, UpdateMeetingCommand request, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<MeetingDto>> UpdateStatus(int id, UpdateMeetingStatusCommand request, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteMeetingCommand(id), cancellationToken);
        return NoContent();
    }
}
