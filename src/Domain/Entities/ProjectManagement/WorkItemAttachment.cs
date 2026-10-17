using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class WorkItemAttachment : BaseAudiTableEntity
{
    public int WorkItemId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
}
