using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiAppSettingRepository : BaseApiService<AppSetting>
    {
        public ApiAppSettingRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "AppSettings";
    }
}
