using System.Text.Json;
using Application.Contracts;
using Domain.Entities.Billing;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Billing;

public record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;
public record GetCurrentSubscriptionQuery : IRequest<SubscriptionDto?>;
