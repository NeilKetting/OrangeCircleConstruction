using System;
using System.Collections.Generic;
using System.Linq;

namespace OCC.Shared.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public OrderType OrderType { get; set; } = OrderType.PurchaseOrder;
        
        // --- Entities Linkage ---
        
        // Purchase Order
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty; // Denormalized or Snapshot Name
        
        // Sales Order
        public Guid? CustomerId { get; set; }

        // --- Snapshot Data (Address/VAT/Phone at time of order) ---
        // This is generic to support both Supplier (PO) and Customer (SO)
        public string EntityAddress { get; set; } = string.Empty; 
        public string EntityTel { get; set; } = string.Empty;
        public string EntityVatNo { get; set; } = string.Empty;
        
        // --- Logistics ---
        public OrderDestinationType DestinationType { get; set; }
        public Guid? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string Attention { get; set; } = string.Empty; // "Att: Jenna" linked to Project Contact

        // --- Financials ---
        public decimal TaxRate { get; set; } = 0.15m; // 15% VAT default
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public string Notes { get; set; } = string.Empty;

        // --- Content ---
        public List<OrderLine> Lines { get; set; } = new();

        // --- Validation / Display Helpers ---
        public string DestinationDisplay => DestinationType == OrderDestinationType.Site 
            ? $"Site: {ProjectName}" 
            : "Office Stock";

        public int TotalItems => Lines?.Count ?? 0;

        // Calculated Totals
        public decimal SubTotal => Lines?.Sum(l => l.LineTotal) ?? 0;
        public decimal VatTotal => Lines?.Sum(l => l.VatAmount) ?? 0;
        public decimal TotalAmount => SubTotal + VatTotal; 
    }

    public enum OrderType
    {
        PurchaseOrder,     // Buying from Supplier
        SalesOrder,        // Selling to Customer
        ReturnToInventory  // Returning leftover material from Project to Stock
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
