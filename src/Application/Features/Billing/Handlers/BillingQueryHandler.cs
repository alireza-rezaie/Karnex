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

    public async Task<PlanDto> Handle(CreatePlanCommand r, CancellationToken ct)
    {
        Validate(r.Name, r.PriceMonthly, r.PriceYearly, r.Users, r.Storage);
        if (await Plans.AnyAsync(x => x.Name == r.Name.Trim(), ct)) throw new BadRequestEntityException("پلنی با این نام وجود دارد.");
        var plan = new Plan { Name = r.Name.Trim(), Description = r.Description?.Trim() ?? "", PriceMonthly = r.PriceMonthly, PriceYearly = r.PriceYearly, Users = r.Users, FeaturesJson = JsonSerializer.Serialize(r.Features ?? []), Highlighted = r.Highlighted, Storage = r.Storage.Trim(), Active = r.Active, DisplayOrder = (await Plans.MaxAsync(x => (int?)x.DisplayOrder, ct) ?? 0) + 1 };
        await unitOfWork.Repository<Plan>().AddAsync(plan, ct); await unitOfWork.Save(ct); return ToDto(plan);
    }

public async Task<SubscriptionDto?> Handle(GetCurrentSubscriptionQuery r, CancellationToken ct)
    { var item = await Subscriptions.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == RequireUser(), ct); return item is null ? null : ToDto(item); }

private string RequireUser() => !string.IsNullOrWhiteSpace(currentUser.UserId) ? currentUser.UserId : throw new BadRequestEntityException("ورود کاربر الزامی است.");
    private static void Validate(string name, decimal monthly, decimal yearly, int users, string storage) { if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(storage) || monthly < 0 || yearly < 0 || users < -1) throw new BadRequestEntityException("مشخصات پلن نامعتبر است."); }
    private static PlanDto ToDto(Plan x) => new(x.Id, x.Name, x.Description, x.PriceMonthly, x.PriceYearly, x.Users, JsonSerializer.Deserialize<List<string>>(x.FeaturesJson) ?? [], x.Highlighted, x.Storage, x.Active, x.DisplayOrder);
    private static SubscriptionDto ToDto(UserSubscription x) => new(x.Id, x.PlanId, x.Status, x.PeriodStart, x.PeriodEnd, x.CancelAtPeriodEnd);
}
