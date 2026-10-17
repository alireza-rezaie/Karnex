namespace Application.Dtos.ProjectManagement;

public record ReminderDto(int Id, string Title, string? Description, DateTime ReminderAt, string Priority, bool IsCompleted, string? UserId, int? ProjectId, int? WorkItemId);
