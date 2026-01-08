using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OCC.Client.DevelopmentToBeDeleted
{
    public partial class BetaNoticeViewModel : ViewModels.Core.ViewModelBase
    {
        public string VersionText { get; }
        private readonly string _rawVersion;

        public event Action? Accepted;

        public BetaNoticeViewModel(string version)
        {
            _rawVersion = version;
            VersionText = $"Version: {version} (BETA)";
        }
        
        // Parameterless for Design Time
        public BetaNoticeViewModel() : this("1.0.0-DEV") { }

        [RelayCommand]
        private void Accept()
        {
            // Save acceptance
            try
            {
                var path = GetAcceptanceFilePath();
                File.WriteAllText(path, _rawVersion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save beta acceptance: {ex.Message}");
            }

            Accepted?.Invoke();
        }

        [RelayCommand]
        private void Cancel()
        {
            // close app
            Environment.Exit(0);
        }

        public static bool IsNoticeAccepted(string currentVersion)
        {
            try
            {
                var path = GetAcceptanceFilePath();
                if (!File.Exists(path)) return false;

                var savedVersion = File.ReadAllText(path).Trim();
                // Compare saved string with current passed version
                return string.Equals(savedVersion, currentVersion, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string GetAcceptanceFilePath()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OCC_Client");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            return Path.Combine(folder, "beta_accepted.txt");
        }
    }
}
