using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reminders;

public sealed class ReminderQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRemindersQuery, IReadOnlyList<ReminderDto>>, IRequestHandler<GetReminderByIdQuery, ReminderDto>
{
private DbSet<Reminder> Reminders => unitOfWork.dbContext.Set<Reminder>();
public async Task<IReadOnlyList<ReminderDto>> Handle(GetRemindersQuery request, CancellationToken cancellationToken)
    {
        var query = Reminders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.UserId)) query = query.Where(x => x.UserId == request.UserId);
        if (request.UpcomingOnly) query = query.Where(x => !x.IsCompleted && x.ReminderAt >= DateTime.UtcNow.AddDays(-1));
        return (await query.OrderBy(x => x.ReminderAt).ToListAsync(cancellationToken)).Select(ToDto).ToList();
    }

public async Task<ReminderDto> Handle(GetReminderByIdQuery request, CancellationToken cancellationToken)
        => ToDto(await Reminders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken) ?? throw new NotFoundEntityException());

private static ReminderDto ToDto(Reminder x) => new(x.Id, x.Title, x.Description, x.ReminderAt, x.Priority.ToString().ToLowerInvariant(), x.IsCompleted, x.UserId, x.ProjectId, x.WorkItemId);
}
