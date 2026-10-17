using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class WorkItemLabel : BaseEntity
{
    public int WorkItemId { get; set; }
    public string Name { get; set; } = null!;
    public string Color { get; set; } = "#6b7280";
    public WorkItem WorkItem { get; set; } = null!;
}
