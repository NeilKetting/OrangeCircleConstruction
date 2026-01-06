using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiEmployeeRepository : BaseApiService<Employee>
    {
        public ApiEmployeeRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "Employees";
    }
}
