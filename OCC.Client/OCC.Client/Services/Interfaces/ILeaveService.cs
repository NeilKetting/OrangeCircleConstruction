using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(int year);
        Task<IEnumerable<LeaveRequest>> GetEmployeeRequestsAsync(Guid employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync();
        
        Task SubmitRequestAsync(LeaveRequest request);
        Task ApproveRequestAsync(Guid requestId, Guid approverId);
        Task RejectRequestAsync(Guid requestId, Guid approverId, string reason);
        
        /// <summary>
        /// Calculates business days between two dates, excluding weekends and public holidays.
        /// </summary>
        Task<int> CalculateBusinessDaysAsync(DateTime start, DateTime end);
    }
}
