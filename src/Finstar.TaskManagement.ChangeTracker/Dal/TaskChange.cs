namespace Finstar.TaskManagement.ChangeTracker.Dal;

public class TaskChange
{
    public Guid EventId { get; set; }
    public Guid TaskId { get; set ; } 
    public string ChangeType { get; set; } 
    public string Title { get; set; } 
    public string Description { get; set; } 
    public DateTimeOffset UpdatedAt { get; set; } 
}