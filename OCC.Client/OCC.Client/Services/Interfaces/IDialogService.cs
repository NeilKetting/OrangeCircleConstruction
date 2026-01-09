using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCC.Client.Services.Interfaces
{
    public interface IDialogService
    {
        Task<string?> PickFileAsync(string title, IEnumerable<string> extensions);
    }
}
