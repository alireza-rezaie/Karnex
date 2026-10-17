using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.ProjectManagement;

public class Project : BaseAudiTableEntity
{
    public string Key { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ProjectType Type { get; set; } = ProjectType.Basic;
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public ProjectPrivacy Privacy { get; set; } = ProjectPrivacy.Private;
    public string Icon { get; set; } = "folder";
    public string Color { get; set; } = "#3b82f6";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
