using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly IRepository<LeaveRequest> _leaveRepository;
        private readonly IRepository<PublicHoliday> _holidayRepository;

        public LeaveService(
            IRepository<LeaveRequest> leaveRepository,
            IRepository<PublicHoliday> holidayRepository)
        {
            _leaveRepository = leaveRepository;
            _holidayRepository = holidayRepository;
        }

        public async Task<IEnumerable<PublicHoliday>> GetPublicHolidaysAsync(int year)
        {
            // Simple fetch, might want to cache or filter by year in Repo if supported
            var all = await _holidayRepository.GetAllAsync();
            return all.Where(h => h.Date.Year == year).OrderBy(h => h.Date);
        }

        public async Task<IEnumerable<LeaveRequest>> GetEmployeeRequestsAsync(Guid employeeId)
        {
            var all = await _leaveRepository.GetAllAsync();
            return all.Where(r => r.EmployeeId == employeeId).OrderByDescending(r => r.StartDate);
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsAsync()
        {
            var all = await _leaveRepository.GetAllAsync();
            return all.Where(r => r.Status == LeaveStatus.Pending).OrderBy(r => r.StartDate);
        }

        public async Task SubmitRequestAsync(LeaveRequest request)
        {
            // Calculate days before submitting
            request.NumberOfDays = await CalculateBusinessDaysAsync(request.StartDate, request.EndDate);
            await _leaveRepository.AddAsync(request);
        }

        public async Task ApproveRequestAsync(Guid requestId, Guid approverId)
        {
            var request = await _leaveRepository.GetByIdAsync(requestId);
            if (request != null)
            {
                request.Status = LeaveStatus.Approved;
                request.ApproverId = approverId;
                request.ActionedDate = DateTime.UtcNow;
                await _leaveRepository.UpdateAsync(request);
            }
        }

        public async Task RejectRequestAsync(Guid requestId, Guid approverId, string reason)
        {
            var request = await _leaveRepository.GetByIdAsync(requestId);
            if (request != null)
            {
                request.Status = LeaveStatus.Rejected;
                request.ApproverId = approverId;
                request.ActionedDate = DateTime.UtcNow;
                request.AdminComment = reason;
                await _leaveRepository.UpdateAsync(request);
            }
        }

        public async Task<int> CalculateBusinessDaysAsync(DateTime start, DateTime end)
        {
            if (end < start) return 0;

            var holidays = await _holidayRepository.GetAllAsync();
            var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();

            int businessDays = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;
                if (holidayDates.Contains(date)) continue;
                
                businessDays++;
            }

            return businessDays;
        }
    }
}
