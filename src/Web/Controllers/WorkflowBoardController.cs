using System.Text.Json;
using Domain.Entities.ProjectManagement;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/workflows")]
public class WorkflowBoardController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) => Ok((await db.WorkflowDefinitions.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct)).Select(ToDto));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return item is null ? NotFound() : Ok(ToDto(item));
    }

    [HttpPost("drafts")]
    public async Task<IActionResult> CreateDraft([FromBody] WorkflowWriteDto request, CancellationToken ct)
    {
        var item = new WorkflowDefinitionRecord { Name = request.Name.Trim(), Description = request.Description?.Trim(), OwnerProjectId = request.OwnerProjectId, IsDraft = true, Version = 0, PayloadJson = request.Payload.GetRawText() };
        db.WorkflowDefinitions.Add(item); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { id = item.Id }, ToDto(item));
    }

    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id, [FromBody] WorkflowCloneDto request, CancellationToken ct)
    {
        var source = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct); if (source is null) return NotFound();
        var item = new WorkflowDefinitionRecord { Name = string.IsNullOrWhiteSpace(request.Name) ? $"{source.Name} (copy)" : request.Name.Trim(), Description = request.Description ?? source.Description, OwnerProjectId = request.OwnerProjectId, IsDraft = true, Version = 0, PayloadJson = source.PayloadJson };
        db.WorkflowDefinitions.Add(item); await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPut("{id:int}/draft")]
    public async Task<IActionResult> SaveDraft(int id, [FromBody] WorkflowWriteDto request, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound();
        item.Name = request.Name.Trim(); item.Description = request.Description?.Trim(); item.PayloadJson = request.Payload.GetRawText(); item.IsDraft = true; item.lastModified = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, [FromBody] JsonElement payload, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct); if (item is null) return NotFound();
        item.PayloadJson = payload.GetRawText(); item.IsDraft = false; item.Version++; item.lastModified = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPut("projects/{projectId:int}/assignment")]
    public async Task<IActionResult> Assign(int projectId, [FromBody] WorkflowAssignmentDto request, CancellationToken ct)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == projectId, ct) || !await db.WorkflowDefinitions.AnyAsync(x => x.Id == request.WorkflowId && !x.IsDraft, ct)) return BadRequest();
        var setting = await db.ProjectWorkflowBoardSettings.FirstOrDefaultAsync(x => x.ProjectId == projectId, ct) ?? new ProjectWorkflowBoardSetting { ProjectId = projectId };
        setting.WorkflowDefinitionRecordId = request.WorkflowId; if (setting.Id == 0) db.ProjectWorkflowBoardSettings.Add(setting); await db.SaveChangesAsync(ct); return Ok(ToSettingDto(setting));
    }

    [HttpGet("projects/{projectId:int}/board")]
    public async Task<IActionResult> GetBoard(int projectId, CancellationToken ct)
    {
        var setting = await db.ProjectWorkflowBoardSettings.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == projectId, ct);
        return Ok(setting is null ? new { projectId, workflowId = (int?)null, kanbanBacklogEnabled = true, columns = JsonDocument.Parse("[]").RootElement } : ToSettingDto(setting));
    }

    [HttpPut("projects/{projectId:int}/board")]
    public async Task<IActionResult> SaveBoard(int projectId, [FromBody] BoardWriteDto request, CancellationToken ct)
    {
        var setting = await db.ProjectWorkflowBoardSettings.FirstOrDefaultAsync(x => x.ProjectId == projectId, ct) ?? new ProjectWorkflowBoardSetting { ProjectId = projectId };
        setting.BoardColumnsJson = request.Columns.GetRawText(); setting.KanbanBacklogEnabled = request.KanbanBacklogEnabled; if (setting.Id == 0) db.ProjectWorkflowBoardSettings.Add(setting); await db.SaveChangesAsync(ct); return Ok(ToSettingDto(setting));
    }

    static object ToDto(WorkflowDefinitionRecord x) => new { id = x.Id, x.Name, x.Description, x.OwnerProjectId, x.IsSystem, x.IsDraft, x.Version, payload = JsonDocument.Parse(x.PayloadJson).RootElement };
    static object ToSettingDto(ProjectWorkflowBoardSetting x) => new { projectId = x.ProjectId, workflowId = x.WorkflowDefinitionRecordId, x.KanbanBacklogEnabled, columns = JsonDocument.Parse(x.BoardColumnsJson).RootElement };
}
public record WorkflowWriteDto(string Name, string? Description, int? OwnerProjectId, JsonElement Payload);
public record WorkflowCloneDto(string? Name, string? Description, int? OwnerProjectId);
public record WorkflowAssignmentDto(int WorkflowId);
public record BoardWriteDto(JsonElement Columns, bool KanbanBacklogEnabled);
