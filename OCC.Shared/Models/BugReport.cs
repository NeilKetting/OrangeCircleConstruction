using System;
using System.ComponentModel.DataAnnotations;

namespace OCC.Shared.Models
{
    public class BugReport
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ReporterId { get; set; }

        [Required]
        public string ReporterName { get; set; } = string.Empty;

        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string ViewName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public string? AdminComments { get; set; }
    }
}
