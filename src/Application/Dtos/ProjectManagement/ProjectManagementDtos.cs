namespace Application.Dtos.ProjectManagement;

public record LabelDto(string Name, string? Color);
public record WorkItemDto(int Id, string Key, string Title, string Description, string Type, string Status, string Priority, string? AssigneeId, string? ReporterId, int ProjectId, int? SprintId, int? EpicId, IReadOnlyList<string> Labels, int? StoryPoints, DateTime? DueDate, DateTime CreatedAt, DateTime? UpdatedAt, int Comments, int Attachments, int Subtasks, int CompletedSubtasks);
public record DashboardTaskStatsDto(int TotalTasks, int InProgressTasks, int CompletedTasks, int OverdueTasks);
public record ProjectDto(int Id, string Key, string Name, string Description, string Type, string Status, string Privacy, string Icon, string Color, int Progress, DateTime? StartDate, DateTime? EndDate, int TasksCount, int CompletedTasks);
public record ProjectMemberDto(string UserId, string Role);
public record ProjectDetailsDto(int Id, string Key, string Name, string Description, string Type, string Status, string Privacy, string Icon, string Color, int Progress, DateTime? StartDate, DateTime? EndDate, int TasksCount, int CompletedTasks, IReadOnlyList<ProjectMemberDto> Members);
public record SprintDto(int Id, int ProjectId, string Name, string Goal, string Status, DateTime StartDate, DateTime EndDate, int Duration, IReadOnlyList<int> TaskIds);
