using Application.Dtos.Account;
using Application.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ManagedUserDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken) => Ok(await Mediator.Send(new GetManagedUsersQuery(search), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ManagedUserDto>> Create(CreateManagedUserCommand request, CancellationToken cancellationToken)
    {
        var user = await Mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManagedUserDto>> Update(string id, UpdateManagedUserCommand request, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpPatch("{id}/role")]
    public async Task<ActionResult<ManagedUserDto>> ChangeRole(string id, ChangeManagedUserRoleCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));

    [HttpPatch("{id}/lock")]
    public async Task<ActionResult<ManagedUserDto>> SetLock(string id, SetManagedUserLockCommand request, CancellationToken cancellationToken) => Ok(await Mediator.Send(request with { Id = id }, cancellationToken));
}
