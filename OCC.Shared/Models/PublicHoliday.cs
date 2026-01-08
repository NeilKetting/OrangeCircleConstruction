using System;

namespace OCC.Shared.Models
{
    public class PublicHoliday : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public DateTime Date { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Year is useful for quick filtering (e.g. GetHolidays(2025))
        public int Year => Date.Year;
    }
}
