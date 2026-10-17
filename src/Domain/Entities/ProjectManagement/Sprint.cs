using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.ProjectManagement;

public class Sprint : BaseAudiTableEntity
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string Goal { get; set; } = string.Empty;
    public SprintStatus Status { get; set; } = SprintStatus.Planned;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
