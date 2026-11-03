using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Billing;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Billing;

public sealed class BillingQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>, IRequestHandler<GetCurrentSubscriptionQuery, SubscriptionDto?>
{
private DbSet<Plan> Plans => unitOfWork.dbContext.Set<Plan>();
    private DbSet<UserSubscription> Subscriptions => unitOfWork.dbContext.Set<UserSubscription>();
public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery r, CancellationToken ct)
        => (await Plans.AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(ct)).Select(ToDto).ToList();

public async Task<SubscriptionDto?> Handle(GetCurrentSubscriptionQuery r, CancellationToken ct)
    { var item = await Subscriptions.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == RequireUser(), ct); return item is null ? null : ToDto(item); }

private string RequireUser() => !string.IsNullOrWhiteSpace(currentUser.UserId) ? currentUser.UserId : throw new BadRequestEntityException("ورود کاربر الزامی است.");
    private static PlanDto ToDto(Plan x) => new(x.Id, x.Name, x.Description, x.PriceMonthly, x.PriceYearly, x.Users, JsonSerializer.Deserialize<List<string>>(x.FeaturesJson) ?? [], x.Highlighted, x.Storage, x.Active, x.DisplayOrder);
    private static SubscriptionDto ToDto(UserSubscription x) => new(x.Id, x.PlanId, x.Status, x.PeriodStart, x.PeriodEnd, x.CancelAtPeriodEnd);
}
