using System;

namespace OCC.Shared.Models
{
    public class StaffMember
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public StaffRole Role { get; set; } = StaffRole.GeneralWorker;
        public double HourlyRate { get; set; }        
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }

    public enum StaffRole
    {
        GeneralWorker,
        Builder,
        Tiler,
        Painter,
        Electrician,
        Plumber,
        SiteManager,
        OfficeAdmin,
        ExternalContractor
    }
}
