using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sprints;

public sealed class SprintCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ChangeSprintStatusCommand, SprintDto>, IRequestHandler<UpdateSprintCommand, SprintDto>, IRequestHandler<DeleteSprintCommand, bool>
{
private DbSet<Sprint> Sprints => unitOfWork.dbContext.Set<Sprint>();
public async Task<SprintDto> Handle(ChangeSprintStatusCommand r, CancellationToken ct) { var sprint = await Sprints.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.Status = r.Status; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(sprint); }

public async Task<SprintDto> Handle(UpdateSprintCommand r, CancellationToken ct) { var sprint = await Sprints.Include(x => x.WorkItems).FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.Name = r.Name.Trim(); sprint.Goal = r.Goal?.Trim() ?? string.Empty; sprint.StartDate = r.StartDate; sprint.EndDate = r.EndDate; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(sprint); }

public async Task<bool> Handle(DeleteSprintCommand r, CancellationToken ct) { var sprint = await Sprints.FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException(); sprint.IsDelete = true; sprint.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return true; }

private static SprintDto ToDto(Sprint x) => new(x.Id, x.ProjectId, x.Name, x.Goal, x.Status.ToString().ToLowerInvariant(), x.StartDate, x.EndDate, Math.Max(1, (int)Math.Ceiling((x.EndDate - x.StartDate).TotalDays / 7)), x.WorkItems.Select(w => w.Id).ToList());
}
