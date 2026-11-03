using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Billing;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Billing;

public record CreatePlanCommand(string Name, string? Description, decimal PriceMonthly, decimal PriceYearly, int Users, IReadOnlyList<string>? Features, bool Highlighted, string Storage, bool Active) : IRequest<PlanDto>;
public record UpdatePlanCommand(int Id, string Name, string? Description, decimal PriceMonthly, decimal PriceYearly, int Users, IReadOnlyList<string>? Features, bool Highlighted, string Storage, bool Active) : IRequest<PlanDto>;
public record ChangeSubscriptionCommand(int PlanId) : IRequest<SubscriptionDto>;
public record CancelSubscriptionCommand(bool Immediate) : IRequest<SubscriptionDto>;
