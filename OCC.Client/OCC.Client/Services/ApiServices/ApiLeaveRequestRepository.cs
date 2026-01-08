using System.Threading.Tasks;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services.ApiServices
{
    public class ApiLeaveRequestRepository : BaseApiService<LeaveRequest>
    {
        public ApiLeaveRequestRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "LeaveRequests";
    }
}
