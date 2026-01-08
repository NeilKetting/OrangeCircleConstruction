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

        public event Action? Accepted;

        public BetaNoticeViewModel()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            VersionText = $"Version: {version} (BETA)";
        }

        [RelayCommand]
        private void Accept()
        {
            // Save acceptance
            try
            {
                var path = GetAcceptanceFilePath();
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                File.WriteAllText(path, currentVersion);
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

        public static bool IsNoticeAccepted()
        {
            try
            {
                var path = GetAcceptanceFilePath();
                if (!File.Exists(path)) return false;

                var savedVersion = File.ReadAllText(path).Trim();
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

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
