using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class HybridAuthService : IAuthService
    {
        private readonly ApiAuthService _apiAuthService;
        private readonly MockAuthService _sqlAuthService;

        public HybridAuthService(ApiAuthService apiAuthService, MockAuthService sqlAuthService)
        {
            _apiAuthService = apiAuthService;
            _sqlAuthService = sqlAuthService;
        }

        private IAuthService CurrentService => ConnectionSettings.Instance.UseApi ? _apiAuthService : _sqlAuthService;

        public User? CurrentUser => CurrentService.CurrentUser;
        public string? AuthToken => CurrentService.AuthToken;
        public bool IsAuthenticated => CurrentService.IsAuthenticated;

        public Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
        {
            return CurrentService.LoginAsync(email, password);
        }

        public Task<bool> RegisterAsync(User user)
        {
            return CurrentService.RegisterAsync(user);
        }

        public Task LogoutAsync()
        {
            return CurrentService.LogoutAsync();
        }
    }
}
