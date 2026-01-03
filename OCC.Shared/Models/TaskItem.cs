namespace OCC.Shared.Models
{
    public class TaskItem
    {
        // Gets or sets the unique identifier of the Task
        public Guid Id { get; set; } = Guid.NewGuid();

        // Gets or sets the unique identifier of the associated project. If null, the task is created as a ToDo for personal record.
        public Guid? ProjectId { get; set; }

        public TaskType Type { get; set; } = TaskType.Task;

        // Name of the Task
        public string Name { get; set; } = string.Empty;

        // Gets or sets the description of the Task
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the planned start date for the task.
        /// </summary>
        #region Planed Info

        public DateTime? PlanedStartDate { get; set; }
        public DateTime? PlanedDueDate { get; set; }
        public TimeSpan? PlanedDurationHours { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the actual date and time when the task was completed.
        /// </summary>
        #region Completed Info

        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompleteDate { get; set; }
        public TimeSpan? ActualDuration { get; set; }

        #endregion

        #region Geofencing Info

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        #endregion
    }

    public enum TaskType
    {
        Task,
        Meeting
    }
}
