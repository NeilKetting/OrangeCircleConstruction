using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IExportService
    {
        Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath);
        Task<string> GenerateHtmlReportAsync<T>(IEnumerable<T> data, string title, Dictionary<string, string> columns);
        Task OpenFileAsync(string filePath);
    }
}
