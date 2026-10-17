using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.ProjectManagement;

public class Reminder : BaseAudiTableEntity
{
    public string? UserId { get; set; }
    public int? ProjectId { get; set; }
    public int? WorkItemId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime ReminderAt { get; set; }
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public bool IsCompleted { get; set; }
    public Project? Project { get; set; }
    public WorkItem? WorkItem { get; set; }
}
