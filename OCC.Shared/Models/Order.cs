using System;
using System.Collections.Generic;

namespace OCC.Shared.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        // Logistics
        public OrderDestinationType DestinationType { get; set; }
        public Guid? ProjectId { get; set; }
        public string? ProjectName { get; set; } // Denormalized for easy display
        
        // Status
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        
        // Content
        public List<OrderLine> Lines { get; set; } = new();
        
        public string Notes { get; set; } = string.Empty;

        // Validation / Display Helpers
        public string DestinationDisplay => DestinationType == OrderDestinationType.Site 
            ? $"Site: {ProjectName}" 
            : "Office Stock";

        public int TotalItems => Lines?.Count ?? 0;
    }

    public enum OrderDestinationType
    {
        Stock,
        Site
    }

    public enum OrderStatus
    {
        Draft,
        Ordered,
        PartialDelivery,
        Completed,
        Cancelled
    }
}
