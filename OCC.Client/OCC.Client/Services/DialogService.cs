using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using OCC.Client.Services.Interfaces;

namespace OCC.Client.Services
{
    public class DialogService : IDialogService
    {
        public async Task<string?> PickFileAsync(string title, IEnumerable<string> extensions)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                if (topLevel == null) return null;

                var options = new FilePickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Supported Files")
                        {
                             Patterns = extensions?.ToList() 
                        }
                    }
                };

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
                return files.FirstOrDefault()?.Path.LocalPath;
            }
            return null;
        }
    }
}
