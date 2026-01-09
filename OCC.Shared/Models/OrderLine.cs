using System;

namespace OCC.Shared.Models
{
    public class OrderLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        
        public string Description { get; set; } = string.Empty; // Product Name
        public string Category { get; set; } = "General";
        
        public double QuantityOrdered { get; set; }
        public double QuantityReceived { get; set; }
        public string UnitOfMeasure { get; set; } = "ea"; // ea, m, kg, m2, m3
        
        // Helper
        public double RemainingQuantity => Math.Max(0, QuantityOrdered - QuantityReceived);
        public bool IsComplete => QuantityReceived >= QuantityOrdered;
    }
}
