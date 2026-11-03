using Application.Dtos.Account;
using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users;

public sealed class UserManagementQueryHandler(UserManager<User> userManager, RoleManager<Role> roleManager, Application.Contracts.IUnitOfWork unitOfWork) : IRequestHandler<GetManagedUsersQuery, IReadOnlyList<ManagedUserDto>>
{

public async Task<IReadOnlyList<ManagedUserDto>> Handle(GetManagedUsersQuery request, CancellationToken ct)
    {
        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search)) query = query.Where(x => x.DisplayName.Contains(request.Search) || x.Email.Contains(request.Search) || x.PhoneNumber.Contains(request.Search));
        var users = await query.OrderBy(x => x.DisplayName).ToListAsync(ct);
        // UserManager and the unit of work share the scoped DbContext.  Build
        // the DTOs sequentially so their role/task/project queries cannot run
        // concurrently on that context.
        var result = new List<ManagedUserDto>(users.Count);
        foreach (var user in users) result.Add(await ToDto(user, ct));
        return result;
    }

private async Task<ManagedUserDto> ToDto(User user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var taskCount = await unitOfWork.dbContext.Set<WorkItem>().CountAsync(x => x.AssigneeId == user.Id && x.Status != Domain.Enums.WorkItemStatus.Done, ct);
        var projectCount = await unitOfWork.dbContext.Set<ProjectMember>().CountAsync(x => x.UserId == user.Id, ct);
        return new(user.Id, user.DisplayName ?? user.UserName ?? "کاربر", user.Email, user.PhoneNumber, user.UserName, roles.ToList(), taskCount, projectCount, user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow);
    }
}
