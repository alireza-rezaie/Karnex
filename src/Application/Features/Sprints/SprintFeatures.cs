using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sprints;

public record GetSprintsQuery(int? ProjectId) : IRequest<IReadOnlyList<SprintDto>>;
public record CreateSprintCommand(int ProjectId, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;
public record UpdateSprintCommand(int Id, string Name, string? Goal, DateTime StartDate, DateTime EndDate) : IRequest<SprintDto>;
public record ChangeSprintStatusCommand(int Id, SprintStatus Status) : IRequest<SprintDto>;
public record DeleteSprintCommand(int Id) : IRequest<bool>;

public class SprintHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSprintsQuery, IReadOnlyList<SprintDto>>, IRequestHandler<CreateSprintCommand, SprintDto>, IRequestHandler<UpdateSprintCommand, SprintDto>, IRequestHandler<ChangeSprintStatusCommand, SprintDto>, IRequestHandler<DeleteSprintCommand, bool>
{
    private DbSet<Sprint> Sprints => unitOfWork.dbContext.Set<Sprint>();
    public async Task<IReadOnlyList<SprintDto>> Handle(GetSprintsQuery r, CancellationToken ct) => (await Sprints.AsNoTracking().Where(x => !r.ProjectId.HasValue || x.ProjectId == r.ProjectId).Include(x => x.WorkItems).OrderByDescending(x => x.StartDate).ToListAsync(ct)).Select(ToDto).ToList();
    public async Task<SprintDto> Handle(CreateSprintCommand r, CancellationToken ct)
    {
        if (!await unitOfWork.dbContext.Set<Project>().AnyAsync(x => x.Id == r.ProjectId, ct)) throw new BadRequestEntityException("پروژه انتخاب‌شده وجود ندارد.");
        var sprint = new Sprint { ProjectId = r.ProjectId, Name = r.Name.Trim(), Goal = r.Goal?.Trim() ?? string.Empty, StartDate = r.StartDate, EndDate = r.EndDate };
        await unitOfWork.Repository<Sprint>().AddAsync(sprint, ct); await unitOfWork.Save(ct); return ToDto(sprint);
    }
    public async Task<SprintDto> Handle(ChangeSprintStatusCommand r, CancellationToken ct) { var sprint = await Sprints.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.Status = r.Status; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(sprint); }
    public async Task<SprintDto> Handle(UpdateSprintCommand r, CancellationToken ct) { var sprint = await Sprints.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.Name = r.Name.Trim(); sprint.Goal = r.Goal?.Trim() ?? string.Empty; sprint.StartDate = r.StartDate; sprint.EndDate = r.EndDate; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(sprint); }
    public async Task<bool> Handle(DeleteSprintCommand r, CancellationToken ct) { var sprint = await Sprints.FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.IsDelete = true; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return true; }
    private static SprintDto ToDto(Sprint x) => new(x.Id, x.ProjectId, x.Name, x.Goal, x.Status.ToString().ToLowerInvariant(), x.StartDate, x.EndDate, Math.Max(1, (int)Math.Ceiling((x.EndDate - x.StartDate).TotalDays / 7)), x.WorkItems.Select(w => w.Id).ToList());
}
