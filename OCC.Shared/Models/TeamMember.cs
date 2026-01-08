using System;

namespace OCC.Shared.Models
{
    public class TeamMember : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TeamId { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Navigation (optional depending on EF setup, but good for shared)
        // public Team? Team { get; set; }
        // public Employee? Employee { get; set; } 
    }
}
