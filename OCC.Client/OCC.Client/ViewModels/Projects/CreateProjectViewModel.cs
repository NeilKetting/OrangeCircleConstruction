using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;
using OCC.Client.Services;

using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Messages;
using System.Linq;

namespace OCC.Client.ViewModels.Projects
{
    public partial class CreateProjectViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<AppSetting> _appSettingsRepository;
        private readonly IRepository<Employee> _staffRepository;

        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler<Guid>? ProjectCreated;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _projectName = string.Empty;

        // Share Options
        [ObservableProperty]
        private bool _isJustMe = true;

        [ObservableProperty]
        private bool _isEveryone = false;

        [ObservableProperty]
        private bool _isSpecific = false;

        [ObservableProperty]
        private bool _isTemplate = false;

        // Settings Expansion
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ModalWidth))]
        private bool _isSettingsVisible;

        // Extended Properties
        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private DateTimeOffset? _targetDate;

        [ObservableProperty]
        private string _projectManager;

        [ObservableProperty]
        private Employee? _siteManager;

        [ObservableProperty]
        private Customer? _customer;

        [ObservableProperty]
        private string _status = "Planning";

        [ObservableProperty]
        private string _priority = "Medium";

        [ObservableProperty]
        private string _shortName = string.Empty;

        // Import State
        [ObservableProperty]
        private bool _isImporting;

        [ObservableProperty]
        private string _importProgressMessage = string.Empty;

        [ObservableProperty]
        private bool _showImportComplete;

        private List<ProjectTask>? _importedTasks;

        #endregion

        #region Properties

        public double ModalWidth => IsSettingsVisible ? 1000 : 700;
        
        // Collections
        public string[] ProjectManagers { get; } = new[] { "Origize63@Gmail.Com (Owner)", "John Doe", "Jane Smith" };

        public System.Collections.ObjectModel.ObservableCollection<Employee> SiteManagers { get; } = new();
        public System.Collections.ObjectModel.ObservableCollection<Customer> Customers { get; } = new();
        public string[] Statuses { get; } = new[] { "Planning", "In Progress", "On Hold", "Completed" };
        public string[] Priorities { get; } = new[] { "low", "Medium", "Important", "Critical" };

        #endregion

        #region Constructors

        public CreateProjectViewModel()
        {
            // Parameterless constructor for design-time support
        }
        
        public CreateProjectViewModel(
            IRepository<Project> projectRepository, 
            IRepository<Customer> customerRepository, 
            IRepository<ProjectTask> taskRepository,
            IRepository<AppSetting> appSettingsRepository,
            IRepository<Employee> staffRepository)
        {
            _projectRepository = projectRepository;
            _customerRepository = customerRepository;
            _taskRepository = taskRepository;
            _appSettingsRepository = appSettingsRepository;
            _staffRepository = staffRepository;

            ProjectManager = ProjectManagers[0];
            
            LoadCustomers();
            LoadSiteManagers();
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task CreateProject()
        {
            if (string.IsNullOrWhiteSpace(ProjectName)) return;

            var newProjectId = Guid.NewGuid();
            var newProject = new Project
            {
                Id = newProjectId,
                Name = ProjectName,
                Description = Description,
                StartDate = DateTime.Now,
                EndDate = TargetDate?.DateTime ?? DateTime.Now.AddMonths(1),
                Status = Status,
                ProjectManager = ProjectManager,
                SiteManagerId = SiteManager?.Id,
                Customer = Customer?.Name ?? string.Empty, 
                Priority = Priority,
                ShortName = string.IsNullOrWhiteSpace(ShortName) ? ProjectName.Substring(0, Math.Min(3, ProjectName.Length)).ToUpper() : ShortName
            };

            // Snapshot Global Work Hours
            try 
            {
                 var settings = await _appSettingsRepository.GetAllAsync();
                 var start = settings.FirstOrDefault(s => s.Key == "WorkStartTime");
                 var end = settings.FirstOrDefault(s => s.Key == "WorkEndTime");
                 var lunch = settings.FirstOrDefault(s => s.Key == "LunchDurationMinutes");

                 if (start != null && TimeSpan.TryParse(start.Value, out var sVal)) newProject.WorkStartTime = sVal;
                 if (end != null && TimeSpan.TryParse(end.Value, out var eVal)) newProject.WorkEndTime = eVal;
                 if (lunch != null && int.TryParse(lunch.Value, out var lVal)) newProject.LunchDurationMinutes = lVal;
            }
            catch (Exception)
            {
                 // CreateProject logic shouldn't fail if settings are missing, defaults apply
            }

            await _projectRepository.AddAsync(newProject);
            
            // Save imported tasks if any
            if (_importedTasks != null && _importedTasks.Count > 0)
            {
                // 1. Flatten and Properties Pass
                var allTasks = new List<ProjectTask>();
                foreach (var rootTask in _importedTasks)
                {
                    FlattenTasks(rootTask, allTasks, 0);
                }

                int orderCounter = 0;
                foreach (var task in allTasks)
                {
                    task.OrderIndex = orderCounter++;
                    task.ProjectId = newProjectId;
                    // Ensure Id is set
                    if (task.Id == Guid.Empty) task.Id = Guid.NewGuid();
                }

                // 2. Save Pass - Only save Roots, EF Core adds children automatically
                foreach (var rootTask in _importedTasks)
                {
                    await _taskRepository.AddAsync(rootTask);
                }
            }

            ProjectCreated?.Invoke(this, newProject.Id);
            WeakReferenceMessenger.Default.Send(new ProjectCreatedMessage(newProject));
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        
        [RelayCommand]
        private void ConfirmImportSave()
        {
            ShowImportComplete = false;
            CreateProjectCommand.Execute(null);
        }

        [RelayCommand]
        private void CancelImportSave()
        {
            ShowImportComplete = false;
            // Keep the data in the form but don't save yet
        }

        [RelayCommand]
        private void StartTemplate()
        {
            // Placeholder
        }

        [RelayCommand]
        private void ToggleSettings()
        {
             IsSettingsVisible = !IsSettingsVisible;
        }

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public async Task ImportProjectAsync(System.IO.Stream stream)
        {
            IsImporting = true;
            ImportProgressMessage = "Starting import...";
            ShowImportComplete = false;

            try
            {
                var parser = new MSProjectXmlParser();
                var progress = new Progress<string>(msg => ImportProgressMessage = msg);

                var result = await parser.ParseAsync(stream, progress);
                
                if (!string.IsNullOrEmpty(result.ProjectName))
                {
                    ProjectName = result.ProjectName;
                }

                if (result.Tasks.Count > 0)
                {
                    // Logic to set dates from tasks if needed
                }

                _importedTasks = result.Tasks;
                
                ImportProgressMessage = "Import Complete!";
                await Task.Delay(500); // UI delay
                ShowImportComplete = true;
            }
            catch (Exception ex)
            {
                ImportProgressMessage = $"Error: {ex.Message}";
                await Task.Delay(2000);
            }
            finally
            {
                IsImporting = false;
            }
        }

        private async void LoadSiteManagers()
        {
            try
            {
                var staff = await _staffRepository.GetAllAsync();
                SiteManagers.Clear();
                foreach (var s in staff)
                {
                    SiteManagers.Add(s);
                }
            }
            catch (Exception) { }
        }

        private async void LoadCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            Customers.Clear();
            foreach (var c in customers)
            {
                Customers.Add(c);
            }
            
            if (Customers.Count == 0)
            {
                Customers.Add(new Customer { Name = "Internal", Id = Guid.NewGuid() });
                Customers.Add(new Customer { Name = "Acme Corp", Id = Guid.NewGuid() });
            }
        }

        #endregion

        #region Helper Methods

        private void FlattenTasks(ProjectTask task, List<ProjectTask> flatList, int level)
        {
            task.IndentLevel = level;
            task.IsGroup = task.Children != null && task.Children.Count > 0;
            
            flatList.Add(task);
            if (task.Children != null && task.Children.Count > 0)
            {
                foreach (var child in task.Children)
                {
                    FlattenTasks(child, flatList, level + 1);
                }

                // Recalculate Summary Dates (Post-Order) to ensure visual containment
                var minStart = DateTime.MaxValue;
                var maxFinish = DateTime.MinValue;
                bool hasDates = false;

                foreach (var child in task.Children)
                {
                     if (child.StartDate < minStart) minStart = child.StartDate;
                     if (child.FinishDate > maxFinish) maxFinish = child.FinishDate;
                     hasDates = true;
                }

                if (hasDates && minStart != DateTime.MaxValue && maxFinish != DateTime.MinValue)
                {
                    task.StartDate = minStart;
                    task.FinishDate = maxFinish;
                    
                    // Update Duration display if needed
                    var days = (maxFinish - minStart).TotalDays;
                    task.Duration = $"{days:0.##} days";
                }
            }
        }

        #endregion
    }
}
