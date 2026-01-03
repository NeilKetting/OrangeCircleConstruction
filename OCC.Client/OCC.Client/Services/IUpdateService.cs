using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public interface IUpdateService
    {
        string CurrentVersion { get; }
        Task<bool> CheckForUpdatesAsync();
        Task DownloadAndInstallUpdateAsync();
    }
}
