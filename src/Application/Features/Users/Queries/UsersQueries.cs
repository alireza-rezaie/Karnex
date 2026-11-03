using Application.Dtos.Account;
using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users;

public record GetManagedUsersQuery(string? Search) : IRequest<IReadOnlyList<ManagedUserDto>>;
