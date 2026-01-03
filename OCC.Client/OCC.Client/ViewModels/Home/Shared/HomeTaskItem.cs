using System;

namespace OCC.Client.ViewModels.Home.Shared
{
    public class HomeTaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsCompleted { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Due { get; set; } = string.Empty;
    }
}
