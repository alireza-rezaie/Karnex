using Domain.Entities.Base;

namespace Domain.Entities.Billing;

public class Plan : BaseAudiTableEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    public int Users { get; set; }
    public string FeaturesJson { get; set; } = "[]";
    public bool Highlighted { get; set; }
    public string Storage { get; set; } = "0 GB";
    public bool Active { get; set; } = true;
    public int DisplayOrder { get; set; }
}

/** Internal subscription state. No payment gateway is called by this entity. */
public class UserSubscription : BaseAudiTableEntity
{
    public string UserId { get; set; } = null!;
    public int PlanId { get; set; }
    public string Status { get; set; } = "active";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public Plan Plan { get; set; } = null!;
}
