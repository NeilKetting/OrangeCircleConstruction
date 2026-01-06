using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Data;
using Microsoft.EntityFrameworkCore;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Shared
{
    public partial class WorkHoursPopupViewModel : ViewModelBase
    {
        #region Private Members

        private readonly AppDbContext _context;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private TimeSpan _startTime = new TimeSpan(8, 0, 0);

        [ObservableProperty]
        private TimeSpan _endTime = new TimeSpan(17, 0, 0);

        [ObservableProperty]
        private int _lunchDurationMinutes = 60;

        #endregion

        #region Constructors

        public WorkHoursPopupViewModel(AppDbContext context)
        {
            _context = context;
            LoadSettings();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Save()
        {
            await SaveSetting("WorkStartTime", StartTime.ToString());
            await SaveSetting("WorkEndTime", EndTime.ToString());
            await SaveSetting("LunchDurationMinutes", LunchDurationMinutes.ToString());

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private async void LoadSettings()
        {
            try
            {
                var workStart = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "WorkStartTime");
                var workEnd = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "WorkEndTime");
                var lunchDur = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "LunchDurationMinutes");

                if (workStart != null && TimeSpan.TryParse(workStart.Value, out var start)) StartTime = start;
                if (workEnd != null && TimeSpan.TryParse(workEnd.Value, out var end)) EndTime = end;
                if (lunchDur != null && int.TryParse(lunchDur.Value, out var lunch)) LunchDurationMinutes = lunch;
            }
            catch (Exception)
            {
                // Fallback to defaults if DB fails or empty
            }
        }

        private async Task SaveSetting(string key, string value)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new AppSetting { Key = key, Value = value };
                _context.AppSettings.Add(setting);
            }
            else
            {
                setting.Value = value;
            }
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
