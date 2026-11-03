using Application.Features.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Route("api/billing")]
public class BillingController : BaseApiController
{
    // Public catalogue used by the pricing/checkout experience.
    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> ListPlans(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPlansQuery(), ct));

    // System administration endpoints are deliberately namespaced and role-gated.
    [Authorize(Roles = "Admin")]
    [HttpGet("/Admin/api/billing/plans")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> ListAdminPlans(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPlansQuery(), ct));

    [Authorize(Roles = "Admin")]
    [HttpPost("/Admin/api/billing/plans")]
    public async Task<ActionResult<PlanDto>> CreatePlan([FromBody] CreatePlanCommand request, CancellationToken ct)
        => Ok(await Mediator.Send(request, ct));

    [Authorize(Roles = "Admin")]
    [HttpPut("/Admin/api/billing/plans/{id:int}")]
    public async Task<ActionResult<PlanDto>> UpdatePlan(int id, [FromBody] UpdatePlanCommand request, CancellationToken ct)
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