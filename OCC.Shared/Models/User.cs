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
