using System;
using System.Collections.Generic;

namespace OCC.Shared.Models
{
    public class Team : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? LeaderId { get; set; } // Optional Team Leader (EmployeeId)

        // Navigation
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    }
}
