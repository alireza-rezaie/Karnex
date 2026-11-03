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

namespace Application.Features.Account.Commands.LoginUser
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, UserDto>
    {
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;


        public LoginCommandHandler(IMapper mapper, SignInManager<User> signInManager, IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _mapper = mapper;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<UserDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.dbContext.Set<User>()
                .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

            if (user == null) throw new BadRequestEntityException("چنین کاربری یافت نشد لطفا ابتدا در سایت ثبت نام کنید");
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) throw new BadRequestEntityException("نام کاربری یا کلمه عبور اشتباه است");

            var mapUser = _mapper.Map<UserDto>(user);
            mapUser.Token = await _tokenService.CreateToken(user);
            return mapUser;
        }
    }
}
