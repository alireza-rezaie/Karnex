using Application.Contracts;
using Web.Extensions;

namespace Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor =  contextAccessor;
        }
        public string UserId => _contextAccessor.HttpContext?.User.GetUserId() ?? string.Empty;

        public string PhoneNumber => _contextAccessor.HttpContext?.User.GetPhoneNumber() ?? string.Empty;
    }
}
