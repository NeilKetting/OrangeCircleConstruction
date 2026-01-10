using System;
using System.Collections.Generic;

namespace OCC.Shared.Models
{
    public class CompanyDetails
    {
        public string CompanyName { get; set; } = "Orange Circle Construction CC";
        public string RegistrationNumber { get; set; } = "2007/004038/23";
        public string VatNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        
        // Physical Address
        public string AddressLine1 { get; set; } = string.Empty;
        public string AddressLine2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "South Africa";

        // Banking
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        
        // Department Emails
        public List<DepartmentEmail> DepartmentEmails { get; set; } = new()
        {
            new DepartmentEmail { Department = "Buying", EmailAddress = "jackie@orange-circle.co.za" },
            new DepartmentEmail { Department = "Accounts", EmailAddress = "anthia@orange-circle.co.za" }
        };

        // Helper
        public string FullAddress => $"{AddressLine1}, {City}, {PostalCode}";
    }

    public class DepartmentEmail
    {
        public string Department { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
