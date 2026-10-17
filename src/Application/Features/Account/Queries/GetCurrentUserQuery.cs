using Application.Contracts;
using Application.Dtos.Account;
using AutoMapper;
using Domain.Entities.Identity;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Account.Queries;

public class GetCurrentUserQuery : IRequest<UserDto>
{
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUserService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId, cancellationToken);

        if (user == null) throw new NotFoundEntityException("کاربر یافت نشد.");

        return _mapper.Map<UserDto>(user);
    }
}
