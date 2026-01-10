using System;

namespace OCC.Shared.Models
{
    public class OrderLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        
        // Link to Inventory (Optional, could be non-inventory item)
        public Guid? InventoryItemId { get; set; }
        
        public string ItemCode { get; set; } = string.Empty; // SKU or Supplier Code
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        
        // Quantity
        public double QuantityOrdered { get; set; }
        public double QuantityReceived { get; set; }
        public string UnitOfMeasure { get; set; } = "ea"; // ea, m, kg, m2, m3
        
        // Pricing (Financials)
        public decimal UnitPrice { get; set; }
        
        // If false, VAT is calculated. If true, Zero Rated (rare but possible).
        // Usually VAT is at Order level, but sometimes per line.
        // We will assume Order TaxRate applies, but we store the calculated amount here.
        public decimal VatAmount { get; set; } 
        
        public decimal LineTotal { get; set; }

        // Helper
        public double RemainingQuantity => Math.Max(0, QuantityOrdered - QuantityReceived);
        public bool IsComplete => QuantityReceived >= QuantityOrdered;
        
        public void CalculateTotal(decimal taxRate)
        {
            // Simple calculation
            decimal qty = (decimal)QuantityOrdered;
            decimal price = UnitPrice;
            
            decimal sub = qty * price;
            VatAmount = sub * taxRate;
            LineTotal = sub; // Usually LineTotal excludes VAT in many systems, or includes. 
                             // Looking at PO image: "Amount" column usually is Excl VAT, then VAT is separate column?
                             // Image shows: Qty | Rate | VAT | Project | Amount
                             // Amount = 0.00. 
                             // Let's assume LineTotal is EXCLUSIVE of VAT, and VAT is additive.
        }
    }
}
