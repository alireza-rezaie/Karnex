using Application.Features.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Route("api/billing")]
public class BillingController : BaseApiController
{
    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> ListPlans(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPlansQuery(), ct));

    [Authorize]
    [HttpPost("plans")]
    public async Task<ActionResult<PlanDto>> CreatePlan(CreatePlanCommand request, CancellationToken ct)
        => Ok(await Mediator.Send(request, ct));

    [Authorize]
    [HttpPut("plans/{id:int}")]
    public async Task<ActionResult<PlanDto>> UpdatePlan(int id, UpdatePlanCommand request, CancellationToken ct)
        => Ok(await Mediator.Send(request with { Id = id }, ct));

    [Authorize]
    [HttpGet("subscription")]
    public async Task<ActionResult<SubscriptionDto?>> GetSubscription(CancellationToken ct)
        => Ok(await Mediator.Send(new GetCurrentSubscriptionQuery(), ct));

    [Authorize]
    [HttpPost("subscription")]
    public async Task<ActionResult<SubscriptionDto>> ChangeSubscription(ChangeSubscriptionCommand request, CancellationToken ct)
        => Ok(await Mediator.Send(request, ct));

    [Authorize]
    [HttpPost("subscription/cancel")]
    public async Task<ActionResult<SubscriptionDto>> CancelSubscription(CancelSubscriptionCommand request, CancellationToken ct)
        => Ok(await Mediator.Send(request, ct));
}