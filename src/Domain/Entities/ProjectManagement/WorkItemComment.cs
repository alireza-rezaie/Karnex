using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class WorkItemComment : BaseAudiTableEntity
{
    public int WorkItemId { get; set; }
    public string AuthorId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public WorkItem WorkItem { get; set; } = null!;
}
