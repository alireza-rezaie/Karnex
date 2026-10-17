using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public string UserId { get; set; } = null!;
    public string Role { get; set; } = "Member";
    public Project Project { get; set; } = null!;
}
