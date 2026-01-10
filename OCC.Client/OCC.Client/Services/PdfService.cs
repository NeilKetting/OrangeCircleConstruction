using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class PdfService : IPdfService
    {
        private readonly ISettingsService _settingsService;

        // Brand Colors
        private static readonly string ColorPrimary = "#EF6C00"; // Orange
        private static readonly string ColorSecondary = "#374151"; // Dark Slate
        private static readonly string ColorLightOrange = "#FFF3E0";
        private static readonly string ColorGreyBorder = "#E5E7EB";

        public PdfService(ISettingsService settingsService)
        {
             _settingsService = settingsService;
             // Configure community license
             QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateOrderPdfAsync(Order order)
        {
            // Fetch company details first (outside background thread to be safe with services)
            var company = await _settingsService.GetCompanyDetailsAsync();

            // Run on background thread to avoid UI freeze
            return await Task.Run(() =>
            {
                // Switch here if you ever need to fallback to Basic (true = Premium, false = Basic)
                bool usePremium = true;

                var doc = Document.Create(container =>
                {
                    if (usePremium)
                    {
                        ComposePremium(container, order, company);
                    }
                    else
                    {
                        ComposeBasic(container, order);
                    }
                });

                // Generate Path (Documents folder)
                string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filename = $"Order_{order.OrderNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string fullPath = Path.Combine(docsPath, filename);

                doc.GeneratePdf(fullPath);
                return fullPath;
            });
        }

        #region Premium Design

        private void ComposePremium(IDocumentContainer container, Order order, CompanyDetails company)
        {
            container.Page(page =>
            {
                page.Margin(0); // Full bleed for header color
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial).FontColor(ColorSecondary));

                page.Header().Element(c => ComposePremiumHeader(c, order, company));
                page.Content().PaddingHorizontal(40).PaddingVertical(20).Element(c => ComposePremiumContent(c, order));
                page.Footer().PaddingHorizontal(40).PaddingBottom(20).Element(c => ComposePremiumFooter(c, company));
            });
        }

        private void ComposePremiumHeader(IContainer container, Order order, CompanyDetails company)
        {
            container.Row(row =>
            {
                // Left: Logo and Address
                row.RelativeItem().PaddingTop(30).PaddingLeft(40).Column(column =>
                {
                    // Logo
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "occ-logo.jpg");
                    if (File.Exists(logoPath))
                    {
                        column.Item().Height(60).Image(logoPath).FitArea();
                    }
                    else
                    {
                        column.Item().Text(company.CompanyName.ToUpper()).FontSize(24).ExtraBold().FontColor(ColorPrimary);
                    }

                    column.Item().PaddingTop(10).Text(company.CompanyName).SemiBold();
                    column.Item().Text(company.FullAddress);
                    column.Item().Text($"Tel: {company.Phone} | Email: {company.Email}").FontSize(9);
                    if(!string.IsNullOrEmpty(company.VatNumber))
                         column.Item().Text($"VAT: {company.VatNumber}").FontSize(9);
                });

                // Right: Purchase Order Strip
                row.RelativeItem().Column(column =>
                {
                    column.Item().Height(30); // Spacer top
                    column.Item().Background(ColorPrimary).PaddingVertical(20).PaddingHorizontal(30).Column(c =>
                    {
                        c.Item().AlignRight().Text(order.OrderType == OrderType.PurchaseOrder ? "PURCHASE ORDER" : "ORDER")
                            .FontSize(24).ExtraBold().FontColor(Colors.White);
                        
                        c.Item().PaddingTop(10).AlignRight().Text(order.OrderNumber)
                            .FontSize(18).SemiBold().FontColor(Colors.White);

                        c.Item().PaddingTop(5).AlignRight().Text($"Date: {order.OrderDate:yyyy-MM-dd}")
                             .FontSize(10).FontColor(Colors.White);
                    });
                });
            });
        }

        private void ComposePremiumContent(IContainer container, Order order)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(20).Row(row =>
                {
                    // Supplier Box
                    row.RelativeItem().Border(1).BorderColor(ColorGreyBorder).Padding(15).Column(c =>
                    {
                        c.Item().Text("VENDOR").FontSize(9).FontColor(Colors.Grey.Medium).SemiBold();
                        c.Item().PaddingTop(5).Text(order.SupplierName ?? "Unknown Supplier").FontSize(12).Bold();
                        
                        if (!string.IsNullOrEmpty(order.EntityAddress)) c.Item().Text(order.EntityAddress);
                        if (!string.IsNullOrEmpty(order.EntityTel)) c.Item().Text($"Tel: {order.EntityTel}");
                        if (!string.IsNullOrEmpty(order.Attention)) c.Item().Text($"Attn: {order.Attention}");
                    });

                    row.ConstantItem(20); // Spacer

                    // Ship To Box
                    row.RelativeItem().Border(1).BorderColor(ColorGreyBorder).Padding(15).Column(c =>
                    {
                        c.Item().Text("SHIP TO / DELIVERY").FontSize(9).FontColor(Colors.Grey.Medium).SemiBold();
                        c.Item().PaddingTop(5).Text(order.DestinationType.ToString()).FontSize(12).Bold();
                        
                        if(order.ExpectedDeliveryDate.HasValue)
                             c.Item().Text($"Expected: {order.ExpectedDeliveryDate:yyyy-MM-dd}");
                    });
                });

                // Order Items Table
                column.Item().PaddingTop(30).Element(c => ComposePremiumTable(c, order));

                // Totals
                column.Item().PaddingTop(20).Row(row => 
                {
                    row.RelativeItem(); // Spacer
                    row.ConstantItem(250).Element(c => ComposePremiumTotals(c, order));
                });
                
                // Signatures
                 column.Item().PaddingTop(40).Row(row =>
                {
                    row.RelativeItem().Column(col => {
                         col.Item().PaddingBottom(5).Text("Authorized Signature").FontSize(9).FontColor(Colors.Grey.Medium);
                         col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });
                    
                    row.ConstantItem(40); // Spacer

                    row.RelativeItem().Column(col => {
                         col.Item().PaddingBottom(5).Text("Received By").FontSize(9).FontColor(Colors.Grey.Medium);
                         col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });
                });
            });
        }

        private void ComposePremiumTable(IContainer container, Order order)
        {
             container.Table(table =>
            {
                // Define Columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.ConstantColumn(80); // Code
                    columns.RelativeColumn();   // Description
                    columns.ConstantColumn(60); // Qty
                    columns.ConstantColumn(50); // Unit
                    columns.ConstantColumn(80); // Rate
                    columns.ConstantColumn(90); // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("#");
                    header.Cell().Element(HeaderStyle).Text("Code");
                    header.Cell().Element(HeaderStyle).Text("Description");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Qty");
                    header.Cell().Element(HeaderStyle).Text("Unit");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Rate");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Total");

                    static IContainer HeaderStyle(IContainer container)
                    {
                        return container.Background(ColorPrimary).PaddingVertical(8).PaddingHorizontal(5).DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                    }
                });

                // Items
                foreach (var line in order.Lines)
                {
                    var index = order.Lines.IndexOf(line);
                    var background = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                    table.Cell().Element(c => CellStyle(c, background)).Text($"{index + 1}");
                    table.Cell().Element(c => CellStyle(c, background)).Text(line.ItemCode);
                    table.Cell().Element(c => CellStyle(c, background)).Text(line.Description);
                    table.Cell().Element(c => CellStyle(c, background)).AlignRight().Text($"{line.QuantityOrdered}");
                    table.Cell().Element(c => CellStyle(c, background)).Text(line.UnitOfMeasure);
                    table.Cell().Element(c => CellStyle(c, background)).AlignRight().Text($"{line.UnitPrice:N2}");
                    table.Cell().Element(c => CellStyle(c, background)).AlignRight().Text($"{line.LineTotal:N2}");
                }

                 static IContainer CellStyle(IContainer container, string bg)
                {
                    return container.Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(8).PaddingHorizontal(5);
                }
            });
        }

        private void ComposePremiumTotals(IContainer container, Order order)
        {
            container.Background(ColorLightOrange).Padding(15).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").SemiBold().FontColor(ColorSecondary);
                    row.RelativeItem().AlignRight().Text($"{order.SubTotal:N2}").FontColor(ColorSecondary);
                });
                
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("VAT (15%):").SemiBold().FontColor(ColorSecondary);
                    row.RelativeItem().AlignRight().Text($"{order.VatTotal:N2}").FontColor(ColorSecondary);
                });

                column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.White);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("TOTAL").FontSize(14).ExtraBold().FontColor(ColorPrimary);
                    row.RelativeItem().AlignRight().Text($"{order.TotalAmount:N2}").FontSize(14).ExtraBold().FontColor(ColorPrimary);
                });
            });
        }
        
        private void ComposePremiumFooter(IContainer container, CompanyDetails company)
        {
             container.Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
                
                row.RelativeItem().AlignRight().Text($"{DateTime.Now:F} - {company.CompanyName} - Confidential");
            });
        }

        #endregion

        #region Basic Design (Backup)

        private void ComposeBasic(IDocumentContainer container, Order order)
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeaderBasic);
                page.Content().Element(c => ComposeContentBasic(c, order));
                page.Footer().Element(ComposeFooterBasic);
            });
        }

        private void ComposeHeaderBasic(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Orange Circle Construction").FontSize(20).SemiBold().FontColor(Colors.Orange.Medium);
                    column.Item().Text("123 Construction Way");
                    column.Item().Text("Cape Town, 8001");
                    column.Item().Text("Tel: +27 21 555 1234");
                    column.Item().Text("Email: orders@orangecircle.co.za");
                });
            });
        }

        private void ComposeContentBasic(IContainer container, Order order)
        {
            container.PaddingVertical(40).Column(column =>
            {
                // Title
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(order.OrderType == OrderType.PurchaseOrder ? "PURCHASE ORDER" : "ORDER").FontSize(24).SemiBold().FontColor(Colors.Grey.Darken3);
                    row.RelativeItem().AlignRight().Text(order.OrderNumber).FontSize(24).SemiBold();
                });

                column.Item().PaddingTop(20).Row(row =>
                {
                    // Supplier Info
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("To:").SemiBold();
                        col.Item().Text(order.SupplierName ?? "Unknown Supplier");
                        if (!string.IsNullOrEmpty(order.EntityAddress))
                        {
                            col.Item().Text(order.EntityAddress);
                        }
                        if (!string.IsNullOrEmpty(order.EntityTel))
                        {
                            col.Item().Text($"Tel: {order.EntityTel}");
                        }
                         if (!string.IsNullOrEmpty(order.Attention))
                        {
                            col.Item().Text($"Attn: {order.Attention}");
                        }
                    });

                    // Order Info
                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().Text("Order Details:").SemiBold();
                        col.Item().Text($"Date: {order.OrderDate:yyyy-MM-dd}");
                        
                        if(order.ExpectedDeliveryDate.HasValue)
                             col.Item().Text($"Due: {order.ExpectedDeliveryDate:yyyy-MM-dd}");

                         col.Item().Text($"Dest: {order.DestinationType}");
                    });
                });

                // Table
                column.Item().PaddingTop(30).Element(c => ComposeTableBasic(c, order));

                // Totals
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Component(new NotesComponent());
                    row.ConstantItem(200).Component(new TotalsComponent(order));
                });
                
                // Signatures
                 column.Item().PaddingTop(50).Row(row =>
                {
                    row.RelativeItem().Column(col => {
                         col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                         col.Item().Text("Approved By").FontSize(10);
                    });
                    
                    row.ConstantItem(50); // Spacer

                    row.RelativeItem().Column(col => {
                         col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                         col.Item().Text("Received By").FontSize(10);
                    });
                });
            });
        }

        private void ComposeTableBasic(IContainer container, Order order)
        {
            container.Table(table =>
            {
                // Define Columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.ConstantColumn(80); // Code
                    columns.RelativeColumn();   // Description
                    columns.ConstantColumn(60); // Qty
                    columns.ConstantColumn(60); // Unit
                    columns.ConstantColumn(80); // Rate
                    columns.ConstantColumn(80); // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Code");
                    header.Cell().Element(CellStyle).Text("Description");
                    header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                    header.Cell().Element(CellStyle).Text("Unit");
                    header.Cell().Element(CellStyle).AlignRight().Text("Rate");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                // Items
                foreach (var line in order.Lines)
                {
                    table.Cell().Element(CellStyle).Text(order.Lines.IndexOf(line) + 1);
                    table.Cell().Element(CellStyle).Text(line.ItemCode);
                    table.Cell().Element(CellStyle).Text(line.Description);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{line.QuantityOrdered}");
                    table.Cell().Element(CellStyle).Text(line.UnitOfMeasure);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{line.UnitPrice:N2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{line.LineTotal:N2}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        }

        private void ComposeFooterBasic(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
                
                row.RelativeItem().AlignRight().Text($"Generated on {DateTime.Now:F} - Orange Circle Construction");
            });
        }

        #endregion
    }
    
    public class NotesComponent : IComponent
    {
        public void Compose(IContainer container)
        {
            container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Item().Text("Notes:").SemiBold();
                column.Item().Text("Please implement deliveries between 8am and 4pm.");
                column.Item().Text("Quote PO number on all invoices.");
            });
        }
    }

    public class TotalsComponent : IComponent
    {
        private readonly Order _order;
        public TotalsComponent(Order order) => _order = order;

        public void Compose(IContainer container)
        {
             container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:");
                    row.RelativeItem().AlignRight().Text($"{_order.SubTotal:N2}");
                });
                 column.Item().Row(row =>
                {
                    row.RelativeItem().Text("VAT (15%):");
                    row.RelativeItem().AlignRight().Text($"{_order.VatTotal:N2}");
                });
                
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Total:").SemiBold().FontSize(14);
                    row.RelativeItem().AlignRight().Text($"{_order.TotalAmount:N2}").SemiBold().FontSize(14);
                });
            });
        }
    }
}
