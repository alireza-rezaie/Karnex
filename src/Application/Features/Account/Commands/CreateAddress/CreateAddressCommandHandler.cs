using Application.Common.Mapping;
using Application.Contracts;
using Application.Dtos.Account;
using Application.Interfaces;
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

namespace Application.Features.Account.Commands.CreateAddress
{
    public class CreateAddressCommand : IRequest<AddressDto>, IMapFrom<Address>
    {
        public bool IsMain { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string FirsName { get; set; }
        public string FullAddress { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }
        public string PostalCode { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateAddressCommand, Address>();
        }
    }
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
    {
        private readonly IUnitOfWork _unitOfWork;   
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public CreateAddressCommandHandler(UserManager<User> userManager, IMapper mapper, ICurrentUserService currentService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _mapper = mapper;
            _currentUserService = currentService;
            _unitOfWork = unitOfWork;
        }
        public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var user = await _userManager.Users.Include(x => x.Address)
                .FirstOrDefaultAsync(x=>x.Id == userId);
            if (user == null) throw new NotFoundEntityException();
            if (request.IsMain && user.Address.Any())
            {
                //remove is main addresses
                user.Address.ForEach(x => x.IsMain = false);
            }
            if (user.Address.Any()) request.IsMain = true;
            var entity = _mapper.Map<Address>(request);
            entity.UserId = userId; 
            user.Address.Add(entity);

            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded) throw new BadRequestEntityException();
            return _mapper.Map<AddressDto>(entity);


        }
    }
}
