using OCC.Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
// using System.Reflection; // Reflection for generic CSV?
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace OCC.Client.Services
{
    public class ExportService : IExportService
    {
        public async Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath)
        {
            // Simple generic CSV export using properties
            var props = typeof(T).GetProperties();
            var sb = new StringBuilder();

            // Header
            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            // Rows
            foreach (var item in data)
            {
                var values = props.Select(p =>
                {
                    var val = p.GetValue(item)?.ToString() ?? "";
                    // Escape commas
                    if (val.Contains(",")) val = $"\"{val}\"";
                    return val;
                });
                sb.AppendLine(string.Join(",", values));
            }

            await File.WriteAllTextAsync(filePath, sb.ToString());
        }

        public async Task<string> GenerateHtmlReportAsync<T>(IEnumerable<T> data, string title, Dictionary<string, string> columns)
        {
            // columns: Key = PropertyName, Value = Header Title
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><style>");
            sb.AppendLine("body { font-family: sans-serif; padding: 20px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; }");
            sb.AppendLine("h1 { color: #333; }");
            sb.AppendLine("</style></head><body>");
            
            sb.AppendLine($"<h1>{title}</h1>");
            sb.AppendLine($"<p>Generated on: {DateTime.Now}</p>");
            
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            foreach (var header in columns.Values)
            {
                sb.AppendLine($"<th>{header}</th>");
            }
            sb.AppendLine("</tr></thead><tbody>");

            var type = typeof(T);
            foreach (var item in data)
            {
                sb.AppendLine("<tr>");
                foreach (var propName in columns.Keys)
                {
                    var prop = type.GetProperty(propName);
                    var val = prop?.GetValue(item)?.ToString() ?? "";
                    sb.AppendLine($"<td>{val}</td>");
                }
                sb.AppendLine("</tr>");
            }
            
            sb.AppendLine("</tbody></table>");
            sb.AppendLine("</body></html>");

            var tempFile = Path.Combine(Path.GetTempPath(), $"Report_{Guid.NewGuid()}.html");
            await File.WriteAllTextAsync(tempFile, sb.ToString());
            return tempFile;
        }

        public Task OpenFileAsync(string filePath)
        {
            try
            {
                var p = new ProcessStartInfo(filePath) { UseShellExecute = true };
                Process.Start(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening file: {ex.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
