using Domain.Entities.Base;
namespace Domain.Entities.ProjectManagement;
public class UserNotification : BaseAudiTableEntity { public string UserId { get; set; } = null!; public string Title { get; set; } = null!; public string? Body { get; set; } public string Channel { get; set; } = "in_app"; public bool IsRead { get; set; } }
public class NotificationPreference : BaseAudiTableEntity { public string UserId { get; set; } = null!; public string PreferencesJson { get; set; } = "{}"; }
