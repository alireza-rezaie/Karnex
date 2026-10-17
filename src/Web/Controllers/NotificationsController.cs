using System.Text.Json;
using Domain.Entities.ProjectManagement;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Web.Controllers;
[ApiController, Route("api/notifications")]
public class NotificationsController(ApplicationDbContext db) : ControllerBase
{
 [HttpGet("channels")] public IActionResult Channels() => Ok(new[] { "in_app", "email", "push" });
 [HttpGet("feed")] public async Task<IActionResult> Feed([FromQuery] string userId, CancellationToken ct) => Ok((await db.UserNotifications.AsNoTracking().Where(x=>x.UserId==userId).OrderByDescending(x=>x.Created).ToListAsync(ct)).Select(x=>new {x.Id,x.Title,x.Body,x.Channel,x.IsRead,x.Created}));
 [HttpGet("unread-count")] public async Task<IActionResult> Unread([FromQuery] string userId, CancellationToken ct) => Ok(new { count = await db.UserNotifications.CountAsync(x=>x.UserId==userId && !x.IsRead,ct)});
 [HttpGet("preferences")] public async Task<IActionResult> Preferences([FromQuery] string userId, CancellationToken ct) { var p=await db.NotificationPreferences.AsNoTracking().FirstOrDefaultAsync(x=>x.UserId==userId,ct); return Ok(new { userId, preferences=JsonSerializer.Deserialize<object>(p?.PreferencesJson??"{}") }); }
 [HttpPut("preferences")] public async Task<IActionResult> SavePreferences([FromBody] PreferenceDto r,CancellationToken ct) { var p=await db.NotificationPreferences.FirstOrDefaultAsync(x=>x.UserId==r.UserId,ct); if(p is null){p=new NotificationPreference{UserId=r.UserId};db.NotificationPreferences.Add(p);} p.PreferencesJson=r.Preferences.GetRawText(); await db.SaveChangesAsync(ct); return Ok(); }
 [HttpPatch("{id:int}/read")] public async Task<IActionResult> MarkRead(int id,CancellationToken ct){var n=await db.UserNotifications.FindAsync([id],ct);if(n is null)return NotFound();n.IsRead=true;await db.SaveChangesAsync(ct);return NoContent();}
 [HttpPatch("mark-all-read")] public async Task<IActionResult> MarkAll([FromBody] UserDto r,CancellationToken ct){var all=await db.UserNotifications.Where(x=>x.UserId==r.UserId&&!x.IsRead).ToListAsync(ct);all.ForEach(x=>x.IsRead=true);await db.SaveChangesAsync(ct);return NoContent();}
}
public record PreferenceDto(string UserId, JsonElement Preferences); public record UserDto(string UserId);
