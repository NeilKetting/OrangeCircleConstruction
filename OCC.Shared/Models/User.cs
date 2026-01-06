namespace OCC.Shared.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // In a real app, this would be a hash
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? Language { get; set; } = "English";
        public Guid? ApproverId { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
        
        // Simplified for now, in a real app these might be navigation properties
        public UserRole UserRole { get; set; } = UserRole.Guest;
        public string? DisplayName => $"{FirstName} {LastName}".Trim();
    }

    public enum UserRole
    {
        Admin,
        SiteManager,
        ExternalContractor,
        Guest
    }
}
