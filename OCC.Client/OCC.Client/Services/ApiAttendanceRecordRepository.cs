using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiAttendanceRecordRepository : BaseApiService<AttendanceRecord>
    {
        public ApiAttendanceRecordRepository(IAuthService authService) : base(authService)
        {
        }

        protected override string ApiEndpoint => "AttendanceRecords";
    }
}
