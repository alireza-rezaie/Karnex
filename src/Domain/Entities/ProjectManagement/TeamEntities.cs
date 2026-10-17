using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class Team : BaseAudiTableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? LeadId { get; set; }
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
}

public class TeamMember : BaseEntity
{
    public int TeamId { get; set; }
    public string UserId { get; set; } = null!;
    public Team Team { get; set; } = null!;
}

public class ProjectRoleDefinition : BaseAudiTableEntity
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string PermissionsJson { get; set; } = "[]";
}
