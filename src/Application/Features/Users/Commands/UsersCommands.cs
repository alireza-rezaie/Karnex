using Application.Dtos.Account;
using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users;

public record CreateManagedUserCommand(string DisplayName, string Email, string PhoneNumber, string Password, string Role) : IRequest<ManagedUserDto>;
public record UpdateManagedUserCommand(string Id, string DisplayName, string Email, string PhoneNumber, string UserName) : IRequest<ManagedUserDto>;
public record ChangeManagedUserRoleCommand(string Id, string Role) : IRequest<ManagedUserDto>;
public record SetManagedUserLockCommand(string Id, bool IsLocked) : IRequest<ManagedUserDto>;
