using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Meetings;

public sealed class MeetingCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateMeetingCommand, MeetingDto>, IRequestHandler<UpdateMeetingStatusCommand, MeetingDto>, IRequestHandler<DeleteMeetingCommand, bool>
{
private DbSet<Meeting> Meetings => unitOfWork.dbContext.Set<Meeting>();
public async Task<MeetingDto> Handle(UpdateMeetingCommand request, CancellationToken ct)
    {
        ValidateTimes(request.StartsAt, request.EndsAt);
        await ValidateProject(request.ProjectId, ct);
        var meeting = await Meetings.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new NotFoundEntityException();
        meeting.Title = request.Title.Trim(); meeting.Description = request.Description?.Trim(); meeting.Location = request.Location?.Trim(); meeting.MeetingUrl = request.MeetingUrl?.Trim(); meeting.StartsAt = request.StartsAt; meeting.EndsAt = request.EndsAt; meeting.Status = request.Status; meeting.OrganizerId = request.OrganizerId; meeting.ProjectId = request.ProjectId; meeting.lastModified = DateTime.UtcNow;
        unitOfWork.dbContext.Set<MeetingParticipant>().RemoveRange(meeting.Participants);
        meeting.Participants.Clear(); SetParticipants(meeting, request.ParticipantIds);
        await unitOfWork.Save(ct);
        return ToDto(meeting);
    }

public async Task<MeetingDto> Handle(UpdateMeetingStatusCommand request, CancellationToken ct)
    {
        var meeting = await Meetings.Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new NotFoundEntityException();
        meeting.Status = request.Status; meeting.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct);
        return ToDto(meeting);
    }

public async Task<bool> Handle(DeleteMeetingCommand request, CancellationToken ct)
    {
        var meeting = await Meetings.FirstOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new NotFoundEntityException();
        meeting.IsDelete = true; meeting.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct);
        return true;
    }

private async Task ValidateProject(int? projectId, CancellationToken ct)
    {
        if (projectId.HasValue && !await unitOfWork.dbContext.Set<Project>().AnyAsync(x => x.Id == projectId, ct)) throw new BadRequestEntityException("پروژه انتخاب‌شده وجود ندارد.");
    }

    private static void ValidateTimes(DateTime startsAt, DateTime endsAt)
    {
        if (endsAt <= startsAt) throw new BadRequestEntityException("زمان پایان جلسه باید بعد از زمان شروع باشد.");
    }

    private static void SetParticipants(Meeting meeting, IReadOnlyList<string>? participantIds)
    {
        foreach (var userId in (participantIds ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct()) meeting.Participants.Add(new MeetingParticipant { UserId = userId });
    }

    private static MeetingDto ToDto(Meeting x) => new(x.Id, x.Title, x.Description, x.Location, x.MeetingUrl, x.StartsAt, x.EndsAt, x.Status.ToString().ToLowerInvariant(), x.OrganizerId, x.ProjectId, x.Participants.Select(p => p.UserId).ToList());
}
