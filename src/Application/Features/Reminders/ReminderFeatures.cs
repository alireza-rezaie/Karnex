using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Reminders;

public record GetRemindersQuery(string? UserId, bool UpcomingOnly) : IRequest<IReadOnlyList<ReminderDto>>;
public record GetReminderByIdQuery(int Id) : IRequest<ReminderDto>;
public record CreateReminderCommand(string Title, string? Description, DateTime ReminderAt, WorkItemPriority Priority, string? UserId, int? ProjectId, int? WorkItemId) : IRequest<ReminderDto>;
public record UpdateReminderCommand(int Id, string Title, string? Description, DateTime ReminderAt, WorkItemPriority Priority, string? UserId, int? ProjectId, int? WorkItemId, bool IsCompleted) : IRequest<ReminderDto>;
public record CompleteReminderCommand(int Id, bool IsCompleted) : IRequest<ReminderDto>;
public record DeleteReminderCommand(int Id) : IRequest<bool>;

public class ReminderHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRemindersQuery, IReadOnlyList<ReminderDto>>, IRequestHandler<GetReminderByIdQuery, ReminderDto>, IRequestHandler<CreateReminderCommand, ReminderDto>, IRequestHandler<UpdateReminderCommand, ReminderDto>, IRequestHandler<CompleteReminderCommand, ReminderDto>, IRequestHandler<DeleteReminderCommand, bool>
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

    public async Task<ReminderDto> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        await ValidateReferences(request.ProjectId, request.WorkItemId, cancellationToken);
        var reminder = new Reminder { Title = request.Title.Trim(), Description = request.Description?.Trim(), ReminderAt = request.ReminderAt, Priority = request.Priority, UserId = request.UserId, ProjectId = request.ProjectId, WorkItemId = request.WorkItemId, CreatedBy = request.UserId ?? string.Empty };
        await unitOfWork.Repository<Reminder>().AddAsync(reminder, cancellationToken);
        await unitOfWork.Save(cancellationToken);
        return ToDto(reminder);
    }

    public async Task<ReminderDto> Handle(UpdateReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await Reminders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken) ?? throw new NotFoundEntityException();
        await ValidateReferences(request.ProjectId, request.WorkItemId, cancellationToken);
        reminder.Title = request.Title.Trim(); reminder.Description = request.Description?.Trim(); reminder.ReminderAt = request.ReminderAt; reminder.Priority = request.Priority; reminder.UserId = request.UserId; reminder.ProjectId = request.ProjectId; reminder.WorkItemId = request.WorkItemId; reminder.IsCompleted = request.IsCompleted; reminder.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(cancellationToken);
        return ToDto(reminder);
    }

    public async Task<ReminderDto> Handle(CompleteReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await Reminders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken) ?? throw new NotFoundEntityException();
        reminder.IsCompleted = request.IsCompleted; reminder.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(cancellationToken);
        return ToDto(reminder);
    }

    public async Task<bool> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await Reminders.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken) ?? throw new NotFoundEntityException();
        reminder.IsDelete = true; reminder.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(cancellationToken);
        return true;
    }

    private async Task ValidateReferences(int? projectId, int? workItemId, CancellationToken ct)
    {
        if (projectId.HasValue && !await unitOfWork.dbContext.Set<Project>().AnyAsync(x => x.Id == projectId, ct)) throw new BadRequestEntityException("پروژه انتخاب‌شده وجود ندارد.");
        if (workItemId.HasValue && !await unitOfWork.dbContext.Set<WorkItem>().AnyAsync(x => x.Id == workItemId, ct)) throw new BadRequestEntityException("وظیفه انتخاب‌شده وجود ندارد.");
    }

    private static ReminderDto ToDto(Reminder x) => new(x.Id, x.Title, x.Description, x.ReminderAt, x.Priority.ToString().ToLowerInvariant(), x.IsCompleted, x.UserId, x.ProjectId, x.WorkItemId);
}
