using System.Security.Claims;
using System.Text.Json;
using Domain.Entities.ProjectManagement;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Authorize]
[Route("api/workflows")]
public class WorkflowBoardController(ApplicationDbContext db) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        await EnsureDefaultWorkflow(ct);
        var items = await db.WorkflowDefinitions.AsNoTracking()
            .Include(x => x.PublishedVersions).OrderBy(x => x.Name).ToListAsync(ct);
        var projectIds = items.Where(x => x.OwnerProjectId.HasValue).Select(x => x.OwnerProjectId!.Value).Distinct().ToArray();
        var accessible = await ManagedProjectIds(projectIds, ct);
        return Ok(items.Where(x => x.IsSystem || !x.OwnerProjectId.HasValue || accessible.Contains(x.OwnerProjectId.Value)).Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.Include(x => x.PublishedVersions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        if (!item.IsSystem && item.OwnerProjectId.HasValue && !await CanManage(item.OwnerProjectId.Value, ct)) return Forbid();
        return Ok(ToDto(item));
    }

    [HttpPost("drafts")]
    public async Task<IActionResult> CreateDraft([FromBody] WorkflowWriteDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.OwnerProjectId is null ||
            !await CanManage(request.OwnerProjectId.Value, ct)) return Forbid();
        var item = new WorkflowDefinitionRecord {
            Name = request.Name.Trim(), Description = request.Description?.Trim(),
            OwnerProjectId = request.OwnerProjectId, IsDraft = true, Version = 0,
            PayloadJson = NormalizePayload(request.Payload)
        };
        db.WorkflowDefinitions.Add(item); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, ToDto(item));
    }

    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id, [FromBody] WorkflowCloneDto request, CancellationToken ct)
    {
        if (request.OwnerProjectId is null || !await CanManage(request.OwnerProjectId.Value, ct)) return Forbid();
        var source = await db.WorkflowDefinitions.AsNoTracking().Include(x => x.PublishedVersions)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (source is null) return NotFound();
        var payload = source.PublishedVersions.OrderByDescending(x => x.Version).FirstOrDefault()?.PayloadJson ?? source.PayloadJson;
        var item = new WorkflowDefinitionRecord {
            Name = string.IsNullOrWhiteSpace(request.Name) ? $"{source.Name} (copy)" : request.Name.Trim(),
            Description = request.Description ?? source.Description, OwnerProjectId = request.OwnerProjectId,
            IsDraft = true, PayloadJson = payload, Version = 0
        };
        db.WorkflowDefinitions.Add(item); await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPut("{id:int}/draft")]
    public async Task<IActionResult> SaveDraft(int id, [FromBody] WorkflowWriteDto request, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        if (item.IsSystem || !item.OwnerProjectId.HasValue || !await CanManage(item.OwnerProjectId.Value, ct)) return Forbid();
        if (!string.IsNullOrWhiteSpace(request.Name)) item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim(); item.PayloadJson = NormalizePayload(request.Payload);
        item.IsDraft = true; item.lastModified = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, [FromBody] JsonElement payload, CancellationToken ct)
    {
        var item = await db.WorkflowDefinitions.Include(x => x.PublishedVersions).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        if (item.IsSystem || !item.OwnerProjectId.HasValue || !await CanManage(item.OwnerProjectId.Value, ct)) return Forbid();
        var normalized = NormalizePayload(payload);
        if (!IsValidPayload(normalized)) return BadRequest(new { message = "وضعیت‌ها یا انتقال‌های گردش‌کار معتبر نیستند." });
        var nextVersion = item.Version + 1;
        item.PayloadJson = normalized; item.Version = nextVersion; item.IsDraft = false; item.lastModified = DateTime.UtcNow;
        item.PublishedVersions.Add(new WorkflowPublishedVersionRecord {
            Version = nextVersion, PayloadJson = normalized, PublishedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct); return Ok(ToDto(item));
    }

    [HttpPut("projects/{projectId:int}/assignment")]
    public async Task<IActionResult> Assign(int projectId, [FromBody] WorkflowAssignmentDto request, CancellationToken ct)
    {
        if (!await CanManage(projectId, ct)) return Forbid();
        var workflow = await db.WorkflowDefinitions.FirstOrDefaultAsync(x => x.Id == request.WorkflowId && !x.IsDraft && !x.IsDelete, ct);
        if (workflow is null) return BadRequest(new { message = "فقط گردش‌کار منتشرشده قابل تخصیص است." });
        var setting = await db.ProjectWorkflowBoardSettings.FirstOrDefaultAsync(x => x.ProjectId == projectId, ct)
            ?? new ProjectWorkflowBoardSetting { ProjectId = projectId };
        setting.WorkflowDefinitionRecordId = workflow.Id;
        if (setting.Id == 0) db.ProjectWorkflowBoardSettings.Add(setting);
        setting.lastModified = DateTime.UtcNow; await db.SaveChangesAsync(ct);
        return Ok(ToSettingDto(setting));
    }

    [HttpGet("projects/{projectId:int}/board")]
    public async Task<IActionResult> GetBoard(int projectId, CancellationToken ct)
    {
        if (!await IsProjectMember(projectId, ct)) return Forbid();
        var setting = await db.ProjectWorkflowBoardSettings.AsNoTracking().FirstOrDefaultAsync(x => x.ProjectId == projectId, ct);
        return Ok(setting is null ? new { projectId, workflowId = (int?)null, kanbanBacklogEnabled = true, columns = JsonDocument.Parse("[]").RootElement }
            : ToSettingDto(setting));
    }

    [HttpPut("projects/{projectId:int}/board")]
    public async Task<IActionResult> SaveBoard(int projectId, [FromBody] BoardWriteDto request, CancellationToken ct)
    {
        if (!await CanManage(projectId, ct)) return Forbid();
        var setting = await db.ProjectWorkflowBoardSettings.FirstOrDefaultAsync(x => x.ProjectId == projectId, ct)
            ?? new ProjectWorkflowBoardSetting { ProjectId = projectId };
        setting.BoardColumnsJson = request.Columns.GetRawText();
        setting.KanbanBacklogEnabled = request.KanbanBacklogEnabled; setting.lastModified = DateTime.UtcNow;
        if (setting.Id == 0) db.ProjectWorkflowBoardSettings.Add(setting);
        await db.SaveChangesAsync(ct); return Ok(ToSettingDto(setting));
    }

    private async Task EnsureDefaultWorkflow(CancellationToken ct)
    {
        if (await db.WorkflowDefinitions.AnyAsync(x => x.IsSystem && !x.IsDelete, ct)) return;
        const string payload = """{"statuses":[{"id":"todo","name":"برای انجام","color":"#64748b"},{"id":"inprogress","name":"در حال انجام","color":"#3b82f6"},{"id":"review","name":"در انتظار بررسی","color":"#f59e0b"},{"id":"done","name":"انجام‌شده","color":"#22c55e"}],"transitions":[{"id":"transition-todo-inprogress","from":"todo","to":"inprogress"},{"id":"transition-inprogress-review","from":"inprogress","to":"review"},{"id":"transition-inprogress-todo","from":"inprogress","to":"todo"},{"id":"transition-review-done","from":"review","to":"done"},{"id":"transition-review-inprogress","from":"review","to":"inprogress"}]}""";
        db.WorkflowDefinitions.Add(new WorkflowDefinitionRecord { Name = "گردش‌کار پیش‌فرض", Description = "چرخه پایه انجام کار در Karnex", IsSystem = true, IsDraft = false, Version = 1, PayloadJson = payload });
        await db.SaveChangesAsync(ct);
    }

    private async Task<bool> IsProjectMember(int projectId, CancellationToken ct) =>
        !string.IsNullOrWhiteSpace(CurrentUserId) && await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == CurrentUserId, ct);

    private async Task<bool> CanManage(int projectId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return false;
        return await db.ProjectMembers.AnyAsync(x => x.ProjectId == projectId && x.UserId == CurrentUserId &&
            (x.Role.ToLower() == "pm" || x.Role.ToLower() == "sm" || x.Role.ToLower() == "project manager" || x.Role.ToLower() == "scrum master"), ct);
    }

    private async Task<HashSet<int>> ManagedProjectIds(int[] ids, CancellationToken ct)
    {
        if (ids.Length == 0 || string.IsNullOrWhiteSpace(CurrentUserId)) return [];
        return (await db.ProjectMembers.Where(x => ids.Contains(x.ProjectId) && x.UserId == CurrentUserId &&
            (x.Role.ToLower() == "pm" || x.Role.ToLower() == "sm" || x.Role.ToLower() == "project manager" || x.Role.ToLower() == "scrum master"))
            .Select(x => x.ProjectId).ToListAsync(ct)).ToHashSet();
    }

    private static string NormalizePayload(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object) return """{"statuses":[],"transitions":[]}""";
        var statuses = payload.TryGetProperty("statuses", out var s) && s.ValueKind == JsonValueKind.Array ? s : JsonDocument.Parse("[]").RootElement;
        var transitions = payload.TryGetProperty("transitions", out var t) && t.ValueKind == JsonValueKind.Array ? t : JsonDocument.Parse("[]").RootElement;
        return JsonSerializer.Serialize(new { statuses, transitions }, JsonOptions);
    }

    private static bool IsValidPayload(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var status in doc.RootElement.GetProperty("statuses").EnumerateArray())
        {
            var id = status.TryGetProperty("id", out var i) ? i.GetString() : null;
            var name = status.TryGetProperty("name", out var n) ? n.GetString()?.Trim() : null;
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || !ids.Add(id) || !names.Add(name)) return false;
        }
        if (ids.Count == 0) return false;
        var paths = new HashSet<string>();
        foreach (var transition in doc.RootElement.GetProperty("transitions").EnumerateArray())
        {
            var from = transition.TryGetProperty("from", out var f) ? f.GetString() : null;
            var to = transition.TryGetProperty("to", out var t) ? t.GetString() : null;
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || from == to || !ids.Contains(from) || !ids.Contains(to) || !paths.Add($"{from}:{to}")) return false;
        }
        return true;
    }

    private static object ToDto(WorkflowDefinitionRecord x) => new {
        id = x.IsSystem ? "workflow-default" : $"workflow-{x.Id}", recordId = x.Id, name = x.Name, description = x.Description,
        ownerProjectId = x.OwnerProjectId?.ToString(), isSystem = x.IsSystem, isDraft = x.IsDraft,
        version = x.Version, publishedVersionId = x.Version > 0 ? $"workflow-{x.Id}-version-{x.Version}" : null,
        payload = JsonDocument.Parse(x.PayloadJson).RootElement,
        publishedVersions = x.PublishedVersions.OrderBy(v => v.Version).Select(v => new {
            id = $"workflow-{x.Id}-version-{v.Version}", workflowId = x.IsSystem ? "workflow-default" : $"workflow-{x.Id}",
            version = v.Version, publishedAt = v.PublishedAt, payload = JsonDocument.Parse(v.PayloadJson).RootElement
        })
    };

    private static object ToSettingDto(ProjectWorkflowBoardSetting x) => new {
        projectId = x.ProjectId.ToString(), workflowId = x.WorkflowDefinitionRecordId.HasValue ? $"workflow-{x.WorkflowDefinitionRecordId.Value}" : null,
        x.KanbanBacklogEnabled, columns = JsonDocument.Parse(x.BoardColumnsJson).RootElement
    };
}
public record WorkflowWriteDto(string Name, string? Description, int? OwnerProjectId, JsonElement Payload);
public record WorkflowCloneDto(string? Name, string? Description, int? OwnerProjectId);
public record WorkflowAssignmentDto(int WorkflowId);
public record BoardWriteDto(JsonElement Columns, bool KanbanBacklogEnabled);
