using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Billing;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Billing;

public record PlanDto(int Id, string Name, string Description, decimal PriceMonthly, decimal PriceYearly, int Users, IReadOnlyList<string> Features, bool Highlighted, string Storage, bool Active, int Order);
public record SubscriptionDto(int Id, int PlanId, string Status, DateTime PeriodStart, DateTime PeriodEnd, bool CancelAtPeriodEnd);
