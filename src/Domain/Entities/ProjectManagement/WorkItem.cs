using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.ProjectManagement;

public class WorkItem : BaseAudiTableEntity
{
    public int ProjectId { get; set; }
    public int? SprintId { get; set; }
    public int? ParentId { get; set; }
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public WorkItemType Type { get; set; } = WorkItemType.Task;
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Todo;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public string? AssigneeId { get; set; }
    public string? ReporterId { get; set; }
    public int? StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Project Project { get; set; } = null!;
    public Sprint? Sprint { get; set; }
    public WorkItem? Parent { get; set; }
    public ICollection<WorkItem> Subtasks { get; set; } = new List<WorkItem>();
    public ICollection<WorkItemLabel> Labels { get; set; } = new List<WorkItemLabel>();
    public ICollection<WorkItemComment> Comments { get; set; } = new List<WorkItemComment>();
    public ICollection<WorkItemAttachment> Attachments { get; set; } = new List<WorkItemAttachment>();
}
