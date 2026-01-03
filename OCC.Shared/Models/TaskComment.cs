namespace OCC.Shared.Models
{
    public class TaskComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Helper properties for UI
        public string Initials => !string.IsNullOrEmpty(AuthorName) ? 
            string.Join("", AuthorName.Split(' ').Select(n => n[0])).ToUpper() : "??";
    }
}
