using System; // Required for Action<T>
using System.Threading.Tasks;
using Velopack;

namespace OCC.Client.Services
{
    public interface IUpdateService
    {
        string CurrentVersion { get; }
        Task<UpdateInfo?> CheckForUpdatesAsync();
        Task DownloadUpdatesAsync(UpdateInfo newVersion, Action<int> progress);
        void ApplyUpdatesAndExit(UpdateInfo newVersion);
    }
}
