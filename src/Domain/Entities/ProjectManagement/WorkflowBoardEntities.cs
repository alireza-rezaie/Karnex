using Domain.Entities.Base;

namespace Domain.Entities.ProjectManagement;

public class WorkflowDefinitionRecord : BaseAudiTableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? OwnerProjectId { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDraft { get; set; }
    public int Version { get; set; }
    public string PayloadJson { get; set; } = "{}";
}

public class ProjectWorkflowBoardSetting : BaseAudiTableEntity
{
    public int ProjectId { get; set; }
    public int? WorkflowDefinitionRecordId { get; set; }
    public bool KanbanBacklogEnabled { get; set; } = true;
    public string BoardColumnsJson { get; set; } = "[]";
}
