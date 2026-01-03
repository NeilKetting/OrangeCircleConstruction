using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace OCC.Client.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly UpdateManager? _mgr;
        private readonly string _updateUrl = "https://github.com/YourUser/YourRepo/releases"; // TODO: This needs to be configured!

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
                
                 _mgr = new UpdateManager(new SimpleWebSource(_updateUrl));
            }
            catch (Exception)
            {
                // Likely running in debug/not installed
                _mgr = null;
            }
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            if (_mgr == null) return false;
            if (!_mgr.IsInstalled) return false;

            try
            {
                var newVersion = await _mgr.CheckForUpdatesAsync();
                return newVersion != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task DownloadAndInstallUpdateAsync()
        {
             if (_mgr == null) return;
             
             try 
             {
                 var newVersion = await _mgr.CheckForUpdatesAsync();
                 if (newVersion == null) return;

                 await _mgr.DownloadUpdatesAsync(newVersion);
                 _mgr.ApplyUpdatesAndExit(newVersion);
             }
             catch
             {
                 // Handle update error (log it etc)
             }
        }
    }
}
