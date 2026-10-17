namespace Application.Dtos.ProjectManagement;

public record MeetingDto(int Id, string Title, string? Description, string? Location, string? MeetingUrl, DateTime StartsAt, DateTime EndsAt, string Status, string? OrganizerId, int? ProjectId, IReadOnlyList<string> ParticipantIds);
