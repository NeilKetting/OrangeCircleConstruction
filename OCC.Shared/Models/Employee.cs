using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents an employee in the system.
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Unique identifier for the employee.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

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
    /// </summary>
    public enum EmployeeRole
    {
        Office,
        GeneralWorker,
        Builder,
        Tiler,
        Painter,
        Electrician,
        Plumber,
        Supervisor,
        ExternalContractor
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
