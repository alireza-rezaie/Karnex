using System.Security.Claims;
using System.Text.Json;
using Domain.Entities.ProjectManagement;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers;

[ApiController, Authorize, Route("api/notifications")]
public class NotificationsController(ApplicationDbContext db) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("channels")]
    public IActionResult Channels() => Ok(new[] { "in_app", "email", "push" });

    [HttpGet("feed")]
    public async Task<IActionResult> Feed(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var items = await db.UserNotifications.AsNoTracking()
            .Where(x => x.UserId == CurrentUserId)
            .OrderByDescending(x => x.Created)
            .ToListAsync(ct);
        return Ok(items.Select(x => new { x.Id, x.Title, x.Body, x.Channel, x.IsRead, x.Created }));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> Unread(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var count = await db.UserNotifications.CountAsync(x => x.UserId == CurrentUserId && !x.IsRead, ct);
        return Ok(new { count });
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var preferences = await db.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == CurrentUserId, ct);
        return Ok(new
        {
            userId = CurrentUserId,
            preferences = JsonSerializer.Deserialize<object>(preferences?.PreferencesJson ?? "{}")
        });
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] PreferenceDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var preference = await db.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserId == CurrentUserId, ct);
        if (preference is null)
        {
            preference = new NotificationPreference { UserId = CurrentUserId };
            db.NotificationPreferences.Add(preference);
        }

        preference.PreferencesJson = request.Preferences.GetRawText();
        await db.SaveChangesAsync(ct);
        return Ok(new { userId = CurrentUserId });
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var notification = await db.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == CurrentUserId, ct);
        if (notification is null) return NotFound();

        notification.IsRead = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAll(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId)) return Unauthorized();
        var notifications = await db.UserNotifications
            .Where(x => x.UserId == CurrentUserId && !x.IsRead)
            .ToListAsync(ct);
        notifications.ForEach(x => x.IsRead = true);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record PreferenceDto(JsonElement Preferences);