using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Meetings;

public sealed class MeetingQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetMeetingsQuery, IReadOnlyList<MeetingDto>>, IRequestHandler<GetMeetingByIdQuery, MeetingDto>
{
private DbSet<Meeting> Meetings => unitOfWork.dbContext.Set<Meeting>();
public async Task<IReadOnlyList<MeetingDto>> Handle(GetMeetingsQuery request, CancellationToken ct)
    {
        var query = Meetings.AsNoTracking().Include(x => x.Participants).AsQueryable();
        if (request.ProjectId.HasValue) query = query.Where(x => x.ProjectId == request.ProjectId);
        if (request.Date.HasValue)
        {
            var start = request.Date.Value.Date;
            query = query.Where(x => x.StartsAt >= start && x.StartsAt < start.AddDays(1));
        }
        return (await query.OrderBy(x => x.StartsAt).ToListAsync(ct)).Select(ToDto).ToList();
    }

public async Task<MeetingDto> Handle(GetMeetingByIdQuery request, CancellationToken ct)
        => ToDto(await Meetings.AsNoTracking().Include(x => x.Participants).FirstOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new NotFoundEntityException());

    public async Task<MeetingDto> Handle(CreateMeetingCommand request, CancellationToken ct)
    {
        ValidateTimes(request.StartsAt, request.EndsAt);
        await ValidateProject(request.ProjectId, ct);
        var meeting = new Meeting { Title = request.Title.Trim(), Description = request.Description?.Trim(), Location = request.Location?.Trim(), MeetingUrl = request.MeetingUrl?.Trim(), StartsAt = request.StartsAt, EndsAt = request.EndsAt, OrganizerId = request.OrganizerId, ProjectId = request.ProjectId, CreatedBy = request.OrganizerId ?? string.Empty };
        SetParticipants(meeting, request.ParticipantIds);
        await unitOfWork.Repository<Meeting>().AddAsync(meeting, ct);
        await unitOfWork.Save(ct);
        return ToDto(meeting);
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
