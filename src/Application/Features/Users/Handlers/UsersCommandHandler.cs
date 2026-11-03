using Application.Dtos.Account;
using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users;

public sealed class UserManagementCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager, Application.Contracts.IUnitOfWork unitOfWork) : IRequestHandler<CreateManagedUserCommand, ManagedUserDto>, IRequestHandler<UpdateManagedUserCommand, ManagedUserDto>, IRequestHandler<ChangeManagedUserRoleCommand, ManagedUserDto>, IRequestHandler<SetManagedUserLockCommand, ManagedUserDto>
{

public async Task<ManagedUserDto> Handle(CreateManagedUserCommand request, CancellationToken ct)
    {
        if (!await roleManager.RoleExistsAsync(request.Role)) throw new BadRequestEntityException("نقش انتخاب‌شده معتبر نیست.");
        if (await userManager.Users.AnyAsync(x => x.Email == request.Email || x.PhoneNumber == request.PhoneNumber, ct)) throw new BadRequestEntityException("ایمیل یا شماره همراه تکراری است.");
        var user = new User { DisplayName = request.DisplayName.Trim(), Email = request.Email.Trim(), UserName = request.Email.Trim(), PhoneNumber = request.PhoneNumber.Trim(), EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) throw new BadRequestEntityException(result.Errors.First().Description);
        var roleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded) throw new BadRequestEntityException(roleResult.Errors.First().Description);
        return await ToDto(user, ct);
    }

public async Task<ManagedUserDto> Handle(UpdateManagedUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.Id) ?? throw new NotFoundEntityException();
        var email = request.Email.Trim(); var phone = request.PhoneNumber.Trim(); var userName = request.UserName.Trim();
        if (await userManager.Users.AnyAsync(x => x.Id != user.Id && (x.Email == email || x.PhoneNumber == phone || x.UserName == userName), ct)) throw new BadRequestEntityException("ایمیل، شماره همراه یا نام کاربری تکراری است.");
        user.DisplayName = request.DisplayName.Trim();
        var nameResult = await userManager.SetUserNameAsync(user, userName);
        if (!nameResult.Succeeded) throw new BadRequestEntityException(nameResult.Errors.First().Description);
        var emailResult = await userManager.SetEmailAsync(user, email);
        if (!emailResult.Succeeded) throw new BadRequestEntityException(emailResult.Errors.First().Description);
        var phoneResult = await userManager.SetPhoneNumberAsync(user, phone);
        if (!phoneResult.Succeeded) throw new BadRequestEntityException(phoneResult.Errors.First().Description);
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) throw new BadRequestEntityException(updateResult.Errors.First().Description);
        return await ToDto(user, ct);
    }

public async Task<ManagedUserDto> Handle(ChangeManagedUserRoleCommand request, CancellationToken ct)
    {
        if (!await roleManager.RoleExistsAsync(request.Role)) throw new BadRequestEntityException("نقش انتخاب‌شده معتبر نیست.");
        var user = await userManager.FindByIdAsync(request.Id) ?? throw new NotFoundEntityException();
        var roles = await userManager.GetRolesAsync(user);
        var remove = await userManager.RemoveFromRolesAsync(user, roles);
        if (!remove.Succeeded) throw new BadRequestEntityException(remove.Errors.First().Description);
        var add = await userManager.AddToRoleAsync(user, request.Role);
        if (!add.Succeeded) throw new BadRequestEntityException(add.Errors.First().Description);
        return await ToDto(user, ct);
    }

public async Task<ManagedUserDto> Handle(SetManagedUserLockCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.Id) ?? throw new NotFoundEntityException();
        var result = await userManager.SetLockoutEndDateAsync(user, request.IsLocked ? DateTimeOffset.MaxValue : null);
        if (!result.Succeeded) throw new BadRequestEntityException(result.Errors.First().Description);
        return await ToDto(user, ct);
    }

private async Task<ManagedUserDto> ToDto(User user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var taskCount = await unitOfWork.dbContext.Set<WorkItem>().CountAsync(x => x.AssigneeId == user.Id && x.Status != Domain.Enums.WorkItemStatus.Done, ct);
        var projectCount = await unitOfWork.dbContext.Set<ProjectMember>().CountAsync(x => x.UserId == user.Id, ct);
        return new(user.Id, user.DisplayName ?? user.UserName ?? "کاربر", user.Email, user.PhoneNumber, user.UserName, roles.ToList(), taskCount, projectCount, user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow);
    }
}
