using System.Text.Json;
using Domain.Entities.ProjectManagement;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController]
[Route("api/project-roles")]
public class ProjectRolesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("permissions")]
    public IActionResult Permissions() => Ok(PermissionDefinitions);
    [HttpGet("projects/{projectId:int}")]
    public async Task<IActionResult> List(int projectId, CancellationToken ct) => Ok((await db.ProjectRoleDefinitions.AsNoTracking().Where(x => x.ProjectId == projectId).OrderBy(x => x.Name).ToListAsync(ct)).Select(ToRole));
    [HttpGet("projects/{projectId:int}/{id:int}")]
    public async Task<IActionResult> Get(int projectId, int id, CancellationToken ct) { var role = await db.ProjectRoleDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId, ct); return role is null ? NotFound() : Ok(ToRole(role)); }
    [HttpPost("projects/{projectId:int}")]
    public async Task<IActionResult> Create(int projectId, [FromBody] RoleWriteDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        if (await db.ProjectRoleDefinitions.AnyAsync(x => x.ProjectId == projectId && x.Name == request.Name.Trim(), ct)) return Conflict("Role name already exists.");
        var role = new ProjectRoleDefinition { ProjectId = projectId, Name = request.Name.Trim(), Description = request.Description?.Trim() ?? string.Empty, PermissionsJson = JsonSerializer.Serialize(request.Permissions ?? []) };
        db.ProjectRoleDefinitions.Add(role); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { projectId, id = role.Id }, ToRole(role));
    }
    [HttpPut("projects/{projectId:int}/{id:int}")]
    public async Task<IActionResult> Update(int projectId, int id, [FromBody] RoleWriteDto request, CancellationToken ct)
    {
        var role = await db.ProjectRoleDefinitions.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId, ct); if (role is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        role.Name = request.Name.Trim(); role.Description = request.Description?.Trim() ?? string.Empty; if (request.Permissions is not null) role.PermissionsJson = JsonSerializer.Serialize(request.Permissions); role.lastModified = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Ok(ToRole(role));
    }
    [HttpDelete("projects/{projectId:int}/{id:int}")]
    public async Task<IActionResult> Delete(int projectId, int id, CancellationToken ct) { var role = await db.ProjectRoleDefinitions.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId, ct); if (role is null) return NotFound(); role.IsDelete = true; await db.SaveChangesAsync(ct); return NoContent(); }
    private static object ToRole(ProjectRoleDefinition x) => new { x.Id, x.ProjectId, x.Name, x.Description, Permissions = JsonSerializer.Deserialize<bool[][]>(x.PermissionsJson) ?? [] };
    private static readonly object[] PermissionDefinitions = [new { module = "projects", permissions = new[] { "view", "create", "edit", "delete", "manage" } }, new { module = "tasks", permissions = new[] { "view", "create", "edit", "delete", "manage" } }, new { module = "sprints", permissions = new[] { "view", "create", "edit", "delete", "manage" } }, new { module = "reports", permissions = new[] { "view", "create", "edit", "delete", "manage" } }, new { module = "members", permissions = new[] { "view", "create", "edit", "delete", "manage" } }];
}

[ApiController]
[Route("api/teams")]
public class TeamsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok((await db.Teams.AsNoTracking().Include(x => x.Members).OrderBy(x => x.Name).ToListAsync(ct)).Select(ToDto));
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id, CancellationToken ct) { var team = await db.Teams.AsNoTracking().Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id, ct); return team is null ? NotFound() : Ok(ToDto(team)); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] TeamWriteDto request, CancellationToken ct) { if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required."); var team = new Team { Name = request.Name.Trim(), Description = request.Description?.Trim(), LeadId = request.LeadId }; foreach (var id in request.MemberIds?.Distinct() ?? []) team.Members.Add(new TeamMember { UserId = id }); if (!string.IsNullOrWhiteSpace(team.LeadId) && !team.Members.Any(x => x.UserId == team.LeadId)) team.Members.Add(new TeamMember { UserId = team.LeadId }); db.Teams.Add(team); await db.SaveChangesAsync(ct); return CreatedAtAction(nameof(Get), new { id = team.Id }, ToDto(team)); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, [FromBody] TeamWriteDto request, CancellationToken ct) { var team = await db.Teams.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id, ct); if (team is null) return NotFound(); if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required."); team.Name = request.Name.Trim(); team.Description = request.Description?.Trim(); team.LeadId = request.LeadId; team.lastModified = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Ok(ToDto(team)); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) { var team = await db.Teams.FirstOrDefaultAsync(x => x.Id == id, ct); if (team is null) return NotFound(); team.IsDelete = true; await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPost("{id:int}/members/{userId}")] public async Task<IActionResult> AddMember(int id, string userId, CancellationToken ct) { var team = await db.Teams.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id, ct); if (team is null) return NotFound(); if (team.Members.Any(x => x.UserId == userId)) return Conflict(); team.Members.Add(new TeamMember { UserId = userId }); await db.SaveChangesAsync(ct); return Ok(ToDto(team)); }
    [HttpDelete("{id:int}/members/{userId}")] public async Task<IActionResult> RemoveMember(int id, string userId, CancellationToken ct) { var member = await db.TeamMembers.Include(x => x.Team).FirstOrDefaultAsync(x => x.TeamId == id && x.UserId == userId, ct); if (member is null) return NotFound(); if (member.Team.LeadId == userId) member.Team.LeadId = null; db.TeamMembers.Remove(member); await db.SaveChangesAsync(ct); return NoContent(); }
    [HttpPut("{id:int}/lead")] public async Task<IActionResult> SetLead(int id, [FromBody] TeamLeadDto request, CancellationToken ct) { var team = await db.Teams.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id, ct); if (team is null) return NotFound(); if (request.UserId is not null && !team.Members.Any(x => x.UserId == request.UserId)) team.Members.Add(new TeamMember { UserId = request.UserId }); team.LeadId = request.UserId; await db.SaveChangesAsync(ct); return Ok(ToDto(team)); }
    private static object ToDto(Team x) => new { x.Id, x.Name, x.Description, x.LeadId, memberIds = x.Members.Select(m => m.UserId).ToList(), activity = "recently", tone = "blue" };
}

public record RoleWriteDto(string Name, string? Description, bool[][]? Permissions);
public record TeamWriteDto(string Name, string? Description, string? LeadId, IReadOnlyList<string>? MemberIds);
public record TeamLeadDto(string? UserId);
