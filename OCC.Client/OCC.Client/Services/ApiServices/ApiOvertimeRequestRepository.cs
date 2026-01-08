using System.Threading.Tasks;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiOvertimeRequestRepository : BaseApiService<OvertimeRequest>
    {
        public ApiOvertimeRequestRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "OvertimeRequests";
    }
}
