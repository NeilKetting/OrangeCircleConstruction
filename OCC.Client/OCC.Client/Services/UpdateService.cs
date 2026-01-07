using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace OCC.Client.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly UpdateManager? _mgr;
        private readonly string _updateUrl = "https://github.com/NeilKetting/OrangeCircleConstruction";

        public string CurrentVersion
        {
            get
            {
                try
                {
                    return _mgr?.CurrentVersion?.ToString() ?? "1.0.0 (Dev)";
                }
                catch
                {
                    return "1.0.0 (Dev)";
                }
            }
        }

        public UpdateService()
        {
            try
            {
                // We default to a SimpleWebSource. Ideally this is config driven.
                // For GitHub, use new GithubSource("url", "token", prerelease)
                // For now, we will use a placeholder or handle the exception if not installed.
                
                // IMPORTANT: In a real app, passing a URL here is critical.
                // Since the user asked "How do I share", I am assuming they will put it somewhere.
                // I'll leave a TODO or a default that doesn't crash.
                
                // Note: Velopack throws if not installed unless we catch it, 
                // but UpdateManager constructor itself usually is just setup.
                // The actual check throws if no local package.
                
                 // We detect if it's a GitHub URL and use the proper source
                 if (_updateUrl.Contains("github.com"))
                 {
                     _mgr = new UpdateManager(new GithubSource(_updateUrl, null, true));
                 }
                 else
                 {
                     _mgr = new UpdateManager(new SimpleWebSource(_updateUrl));
                 }
            }
            catch (Exception)
            {
                // Likely running in debug/not installed
                _mgr = null;
            }
        }

        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            if (_mgr == null || !_mgr.IsInstalled) return null;

            try
            {
                return await _mgr.CheckForUpdatesAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task DownloadUpdatesAsync(UpdateInfo newVersion, Action<int> progress)
        {
            if (_mgr == null) return;
             
            try 
            {
                await _mgr.DownloadUpdatesAsync(newVersion, progress);
            }
            catch
            {
                // Handle download error
                throw;
            }
        }

        public void ApplyUpdatesAndExit(UpdateInfo newVersion)
        {
             _mgr?.ApplyUpdatesAndRestart(newVersion);
        }
    }
}
