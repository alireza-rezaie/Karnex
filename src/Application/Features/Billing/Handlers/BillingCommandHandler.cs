using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Billing;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Billing;

public sealed class BillingCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser) : IRequestHandler<CreatePlanCommand, PlanDto>, IRequestHandler<UpdatePlanCommand, PlanDto>, IRequestHandler<ChangeSubscriptionCommand, SubscriptionDto>, IRequestHandler<CancelSubscriptionCommand, SubscriptionDto>
{
private DbSet<Plan> Plans => unitOfWork.dbContext.Set<Plan>();
    private DbSet<UserSubscription> Subscriptions => unitOfWork.dbContext.Set<UserSubscription>();
public async Task<PlanDto> Handle(CreatePlanCommand r, CancellationToken ct)
    {
        Validate(r.Name, r.PriceMonthly, r.PriceYearly, r.Users, r.Storage);
        if (await Plans.AnyAsync(x => x.Name == r.Name.Trim(), ct)) throw new BadRequestEntityException("پلنی با این نام وجود دارد.");
        var plan = new Plan { Name = r.Name.Trim(), Description = r.Description?.Trim() ?? "", PriceMonthly = r.PriceMonthly, PriceYearly = r.PriceYearly, Users = r.Users, FeaturesJson = JsonSerializer.Serialize(r.Features ?? []), Highlighted = r.Highlighted, Storage = r.Storage.Trim(), Active = r.Active, DisplayOrder = (await Plans.MaxAsync(x => (int?)x.DisplayOrder, ct) ?? 0) + 1 };
        await unitOfWork.Repository<Plan>().AddAsync(plan, ct); await unitOfWork.Save(ct); return ToDto(plan);
    }

public async Task<PlanDto> Handle(UpdatePlanCommand r, CancellationToken ct)
    {
        Validate(r.Name, r.PriceMonthly, r.PriceYearly, r.Users, r.Storage);
        var plan = await Plans.FirstOrDefaultAsync(x => x.Id == r.Id, ct) ?? throw new NotFoundEntityException();
        if (await Plans.AnyAsync(x => x.Id != r.Id && x.Name == r.Name.Trim(), ct)) throw new BadRequestEntityException("پلنی با این نام وجود دارد.");
        plan.Name = r.Name.Trim(); plan.Description = r.Description?.Trim() ?? ""; plan.PriceMonthly = r.PriceMonthly; plan.PriceYearly = r.PriceYearly; plan.Users = r.Users; plan.FeaturesJson = JsonSerializer.Serialize(r.Features ?? []); plan.Highlighted = r.Highlighted; plan.Storage = r.Storage.Trim(); plan.Active = r.Active; plan.lastModified = DateTime.UtcNow;
        await unitOfWork.Save(ct); return ToDto(plan);
    }

public async Task<SubscriptionDto> Handle(ChangeSubscriptionCommand r, CancellationToken ct)
    {
        var userId = RequireUser(); var plan = await Plans.FirstOrDefaultAsync(x => x.Id == r.PlanId && x.Active, ct) ?? throw new BadRequestEntityException("پلن انتخاب‌شده فعال نیست.");
        var subscription = await Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (subscription is null) { subscription = new UserSubscription { UserId = userId, PlanId = plan.Id, PeriodStart = DateTime.UtcNow, PeriodEnd = DateTime.UtcNow.AddMonths(1) }; await unitOfWork.Repository<UserSubscription>().AddAsync(subscription, ct); }
        else { subscription.PlanId = plan.Id; subscription.Status = "active"; subscription.CancelAtPeriodEnd = false; subscription.PeriodStart = DateTime.UtcNow; subscription.PeriodEnd = DateTime.UtcNow.AddMonths(1); subscription.lastModified = DateTime.UtcNow; }
        await unitOfWork.Save(ct); return ToDto(subscription);
    }

public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand r, CancellationToken ct)
    {
        var subscription = await Subscriptions.FirstOrDefaultAsync(x => x.UserId == RequireUser(), ct) ?? throw new NotFoundEntityException("اشتراک فعالی یافت نشد.");
        subscription.CancelAtPeriodEnd = !r.Immediate; subscription.Status = r.Immediate ? "canceled" : "active"; if (r.Immediate) subscription.PeriodEnd = DateTime.UtcNow; subscription.lastModified = DateTime.UtcNow; await unitOfWork.Save(ct); return ToDto(subscription);
    }

private string RequireUser() => !string.IsNullOrWhiteSpace(currentUser.UserId) ? currentUser.UserId : throw new BadRequestEntityException("ورود کاربر الزامی است.");
    private static void Validate(string name, decimal monthly, decimal yearly, int users, string storage) { if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(storage) || monthly < 0 || yearly < 0 || users < -1) throw new BadRequestEntityException("مشخصات پلن نامعتبر است."); }
    private static PlanDto ToDto(Plan x) => new(x.Id, x.Name, x.Description, x.PriceMonthly, x.PriceYearly, x.Users, JsonSerializer.Deserialize<List<string>>(x.FeaturesJson) ?? [], x.Highlighted, x.Storage, x.Active, x.DisplayOrder);
    private static SubscriptionDto ToDto(UserSubscription x) => new(x.Id, x.PlanId, x.Status, x.PeriodStart, x.PeriodEnd, x.CancelAtPeriodEnd);
}
