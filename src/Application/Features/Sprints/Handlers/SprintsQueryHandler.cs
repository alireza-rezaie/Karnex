using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sprints;

public sealed class SprintQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSprintsQuery, IReadOnlyList<SprintDto>>
{
private DbSet<Sprint> Sprints => unitOfWork.dbContext.Set<Sprint>();
public async Task<IReadOnlyList<SprintDto>> Handle(GetSprintsQuery r, CancellationToken ct) => (await Sprints.AsNoTracking().Where(x => !r.ProjectId.HasValue || x.ProjectId == r.ProjectId).Include(x => x.WorkItems).OrderByDescending(x => x.StartDate).ToListAsync(ct)).Select(ToDto).ToList();

private static SprintDto ToDto(Sprint x) => new(x.Id, x.ProjectId, x.Name, x.Goal, x.Status.ToString().ToLowerInvariant(), x.StartDate, x.EndDate, Math.Max(1, (int)Math.Ceiling((x.EndDate - x.StartDate).TotalDays / 7)), x.WorkItems.Select(w => w.Id).ToList());
}
