using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiUserRepository : BaseApiService<User>
    {
        public ApiUserRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Users";
    }
}
