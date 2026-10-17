using Application.Dtos.ProjectManagement;
using Application.Features.WorkItems;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Route("api/tasks")]
public class WorkItemsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkItemDto>>> GetAll([FromQuery] int? projectId, [FromQuery] int? sprintId, [FromQuery] WorkItemStatus? status, [FromQuery] WorkItemPriority? priority, [FromQuery] string? assigneeId, [FromQuery] string? search, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetWorkItemsQuery(projectId, sprintId, status, priority, assigneeId, search), cancellationToken));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkItemDto>> GetById(int id, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetWorkItemByIdQuery(id), cancellationToken));
    [HttpPost]
    public async Task<ActionResult<WorkItemDto>> Create([FromBody] CreateWorkItemCommand request, CancellationToken cancellationToken) { var item = await Mediator.Send(request, cancellationToken); return CreatedAtAction(nameof(GetById), new { id = item.Id }, item); }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WorkItemDto>> Update(int id, [FromBody] UpdateWorkItemCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<WorkItemDto>> ChangeStatus(int id, [FromBody] ChangeWorkItemStatusCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) { await Mediator.Send(new DeleteWorkItemCommand(id), cancellationToken); return NoContent(); }
    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddWorkItemCommentCommand request, CancellationToken cancellationToken) { await Mediator.Send(request with { Id = id }, cancellationToken); return NoContent(); }
    [HttpPost("{id:int}/attachments")]
    public async Task<IActionResult> AddAttachment(int id, [FromBody] AddWorkItemAttachmentCommand request, CancellationToken cancellationToken) { await Mediator.Send(request with { Id = id }, cancellationToken); return NoContent(); }
    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<DashboardTaskStatsDto>> GetDashboardStats([FromQuery] int? projectId, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetDashboardTaskStatsQuery(projectId), cancellationToken));
    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<WorkItemDto>>> GetMy(CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetMyWorkItemsQuery(), cancellationToken));
}
