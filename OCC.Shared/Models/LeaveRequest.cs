using System;

namespace OCC.Shared.Models
{
    public class LeaveRequest : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Total number of business days for this leave request (excluding weekends/holidays).
        /// </summary>
        public int NumberOfDays { get; set; }

        public LeaveType LeaveType { get; set; } = LeaveType.Annual;

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Comments from the approver/admin (e.g., rejection reason).
        /// </summary>
        public string? AdminComment { get; set; }
        
        /// <summary>
        /// ID of user who approved/rejected the request.
        /// </summary>
        public Guid? ApproverId { get; set; }
        public DateTime? ActionedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// If true, this leave is marked as Unpaid (e.g. insufficient balance).
        /// </summary>
        public bool IsUnpaid { get; set; }
    }

    public enum LeaveType
    {
        Annual,
        Sick,
        FamilyResponsibility,
        Study,
        Maternity,
        Unpaid
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}
