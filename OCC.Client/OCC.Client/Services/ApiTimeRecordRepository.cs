using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiTimeRecordRepository : BaseApiService<TimeRecord>
    {
        public ApiTimeRecordRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "TimeRecords";
    }
}
