using Domain.Entities.Base;
using Domain.Enums;

namespace Domain.Entities.ProjectManagement;

public class Meeting : BaseAudiTableEntity
{
    public int? ProjectId { get; set; }
    public string? OrganizerId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public Project? Project { get; set; }
    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
}

public class MeetingParticipant : BaseEntity
{
    public int MeetingId { get; set; }
    public string UserId { get; set; } = null!;
    public bool HasAccepted { get; set; }
    public Meeting Meeting { get; set; } = null!;
}
