using Application.Contracts;
using Application.Dtos.Account;
using AutoMapper;
using Domain.Entities.Identity;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Account.Queries
{
    public class GetAddressesQuery : IRequest<IEnumerable<AddressDto>>
    {

    }
    public class GetAddressesQueryHandler : IRequestHandler<GetAddressesQuery, IEnumerable<AddressDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public GetAddressesQueryHandler(ICurrentUserService currentUserService,IMapper mapper,UserManager<User> userManager)
        {
            _currentUserService = currentUserService;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<IEnumerable<AddressDto>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.Include(x => x.Address)
                .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId,cancellationToken);
            if (user == null) throw new NotFoundEntityException();
            return _mapper.Map<IEnumerable<AddressDto>>(user.Address);

        }
    }
}
