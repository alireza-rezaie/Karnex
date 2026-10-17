using Application.Dtos.ProjectManagement;
using Application.Features.Reminders;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Route("api/reminders")]
public class RemindersController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReminderDto>>> GetAll([FromQuery] string? userId, [FromQuery] bool upcomingOnly, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetRemindersQuery(userId, upcomingOnly), cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReminderDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetReminderByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ReminderDto>> Create([FromBody] CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await Mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = reminder.Id }, reminder);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReminderDto>> Update(int id, [FromBody] UpdateReminderCommand request, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpPatch("{id:int}/completion")]
    public async Task<ActionResult<ReminderDto>> Complete(int id, [FromBody] CompleteReminderCommand request, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteReminderCommand(id), cancellationToken);
        return NoContent();
    }
}
