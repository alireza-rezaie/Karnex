using Application.Dtos.Account;
using Application.Features.Account.Commands.AccountUser;
using Application.Features.Account.Commands.CreateAddress;
using Application.Features.Account.Commands.LoginUser;
using Application.Features.Account.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Common;

namespace Web.Controllers
{
    public class AccountController : BaseApiController
    {
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterCommand request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken));
        }

        [Authorize]
        [HttpPost("CreateAddress")]
        public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressCommand request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken));
        }

        [Authorize]
        [HttpGet("GetAddress")]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetAddress(CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(new GetAddressesQuery(), cancellationToken));
        }

        [Authorize]
        [HttpGet("CurrentUser")]
        public async Task<ActionResult<UserDto>> CurrentUser(CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(new GetCurrentUserQuery(), cancellationToken));
        }
    }
}