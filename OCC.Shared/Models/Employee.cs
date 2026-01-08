using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents an employee in the system.
    /// </summary>
    /// <summary>
    /// Represents an employee in the system.
    /// </summary>
    public class Employee : IEntity
    {
        /// <summary>
        /// Unique identifier for the employee.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Optional link to a User Login (Identity).
        /// Allows linking this Resource (Employee) to a System User (Login).
        /// </summary>
        public Guid? LinkedUserId { get; set; }

        /// <summary>
        /// Employee's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Employee's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Type of identification document used.
        /// </summary>
        public IdType IdType { get; set; } = IdType.RSAId;

        /// <summary>
        /// Employee's identification number (RSA ID or Passport Number).
        /// </summary>
        public string IdNumber { get; set; } = string.Empty;

        /// <summary>
        /// Contact email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Date of Birth, typically calculated from RSA ID or manually entered.
        /// Defaults to 30 years ago.
        /// </summary>
        public DateTime DoB { get; set; } = DateTime.Now.AddYears(-30);

        /// <summary>
        /// The primary skill set of the employee.
        /// </summary>
        public EmployeeRole Role { get; set; } = EmployeeRole.GeneralWorker;

        /// <summary>
        /// Hourly pay rate in local currency.
        /// </summary>
        public double HourlyRate { get; set; }

        /// <summary>
        /// Computed full name of the employee.
        /// </summary>
        public string DisplayName => $"{FirstName}, {LastName}".Trim();

        /// <summary>
        /// Assigned employee number (e.g., EMP001).
        /// </summary>
        public string EmployeeNumber { get; set; } = string.Empty;

        /// <summary>
        /// Type of employment (Permanent or Contract).
        /// </summary>
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Permanent;

        /// <summary>
        /// Duration of the contract if EmploymentType is Contract (e.g., "6 Months").
        /// This is nullable as it only applies to contract workers.
        /// </summary>
        public string? ContractDuration { get; set; }

        /// <summary>
        /// The date the employee started working.
        /// </summary>
        public DateTime EmploymentDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Valid values: "Johannesburg", "Cape Town"
        /// </summary>
        public string Branch { get; set; } = "Johannesburg";

        /// <summary>
        /// Start time of the employee's shift.
        /// </summary>
        public TimeSpan? ShiftStartTime { get; set; } = new TimeSpan(7, 0, 0);

        /// <summary>
        /// End time of the employee's shift.
        /// </summary>
        public TimeSpan? ShiftEndTime { get; set; } = new TimeSpan(16, 45, 0);

        // Banking Details
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BranchCode { get; set; }
        public string? AccountType { get; set; }
        
        /// <summary>
        /// Determines if the rate is Hourly or a Monthly Salary.
        /// </summary>
        public RateType RateType { get; set; } = RateType.Hourly;
    }

    public enum RateType
    {
        Hourly,
        MonthlySalary
    }

    /// <summary>
    /// Defines the nature of the employment contract.
    /// </summary>
    public enum EmploymentType
    {
        Permanent,
        Contract
    }

    /// <summary>
    /// Defines the role or trade of the employee.
    ///Ids are explicitly assigned to prevent shifting if items are reordered.
    /// </summary>
    public enum EmployeeRole
    {
        // Legacy / Existing Roles
        Office = 0,
        GeneralWorker = 1,
        Builder = 2,
        Tiler = 3,
        Painter = 4,
        Electrician = 5,
        Plumber = 6,
        Supervisor = 7,             // Generic Supervisor (legacy)
        ExternalContractor = 8,

        // New Client Requested Roles
        BuildingSupervisor = 9,
        PlasterSupervisor = 10,
        ShopfittingSupervisor = 11,
        PaintingSupervisor = 12,
        LabourSupervisor = 13,
        Cleaner = 14,
        Shopfitter = 15,
        Plasterer = 16,
        PlasterLabour = 17,
        SeniorForeman = 18,
        Foreman = 19,
        Welder = 20
    }

    /// <summary>
    /// Supported identification document types.
    /// </summary>
    public enum IdType
    {
        RSAId,
        Passport
    }
}
