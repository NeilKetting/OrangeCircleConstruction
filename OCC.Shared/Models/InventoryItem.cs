using System;

namespace OCC.Shared.Models
{
    public class InventoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Location { get; set; } = "Warehouse"; // Rack A, Shelf 1
        
        public double QuantityOnHand { get; set; }
        public double ReorderPoint { get; set; }
        public string UnitOfMeasure { get; set; } = "ea";
        
        // Status
        public InventoryStatus Status => QuantityOnHand <= ReorderPoint ? InventoryStatus.Low : InventoryStatus.OK;
        
        // Alias for View Binding compatibility
        public InventoryStatus InventoryStatus => Status;
    }

    public enum InventoryStatus
    {
        OK,
        Low,
        Critical
    }
}
