using OCC.Shared.Models;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IPdfService
    {
        Task<string> GenerateOrderPdfAsync(Order order);
    }
}
