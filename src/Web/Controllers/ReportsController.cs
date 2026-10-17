using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview([FromQuery] int? projectId, CancellationToken ct)
    {
        var items = db.WorkItems.AsNoTracking(); if (projectId.HasValue) items = items.Where(x => x.ProjectId == projectId);
        var all = await items.ToListAsync(ct); return Ok(new { total = all.Count, done = all.Count(x => x.Status.ToString() == "Done"), inProgress = all.Count(x => x.Status.ToString() == "InProgress"), todo = all.Count(x => x.Status.ToString() == "Todo"), overdue = all.Count(x => x.DueDate < DateTime.UtcNow && x.Status.ToString() != "Done") });
    }
    [HttpGet("burndown")] public async Task<IActionResult> Burndown([FromQuery] int projectId, CancellationToken ct) => Ok((await db.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId).ToListAsync(ct)).GroupBy(x => x.Created.Date).OrderBy(x => x.Key).Select(g => new { date = g.Key, created = g.Count(), completed = g.Count(x => x.CompletedAt != null) }));
    [HttpGet("velocity")] public async Task<IActionResult> Velocity([FromQuery] int projectId, CancellationToken ct) => Ok((await db.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId && x.SprintId != null).ToListAsync(ct)).GroupBy(x => x.SprintId).Select(g => new { sprintId = g.Key, completedPoints = g.Where(x => x.CompletedAt != null).Sum(x => x.StoryPoints ?? 0), totalPoints = g.Sum(x => x.StoryPoints ?? 0) }));
    [HttpGet("cumulative-flow")] public async Task<IActionResult> CumulativeFlow([FromQuery] int projectId, CancellationToken ct) => Ok((await db.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId).ToListAsync(ct)).GroupBy(x => x.Status.ToString()).Select(g => new { status = g.Key, count = g.Count() }));
    [HttpGet("control-chart")] public async Task<IActionResult> ControlChart([FromQuery] int projectId, CancellationToken ct) => Ok((await db.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId && x.CompletedAt != null).ToListAsync(ct)).Select(x => new { key = x.Key, cycleTimeHours = (x.CompletedAt!.Value - x.Created).TotalHours }));
    [HttpGet("created-resolved")] public async Task<IActionResult> CreatedResolved([FromQuery] int projectId, CancellationToken ct) => Ok((await db.WorkItems.AsNoTracking().Where(x => x.ProjectId == projectId).ToListAsync(ct)).GroupBy(x => x.Created.Date).OrderBy(g => g.Key).Select(g => new { date = g.Key, created = g.Count(), resolved = g.Count(x => x.CompletedAt != null) }));
    [HttpPost("exports")] public IActionResult Export([FromBody] ExportRequest request) => Accepted(new { id = Guid.NewGuid().ToString("N"), status = "queued", format = request.Format, report = request.Report, requestedAt = DateTime.UtcNow });
}
public record ExportRequest(string Report, string Format, int? ProjectId);
