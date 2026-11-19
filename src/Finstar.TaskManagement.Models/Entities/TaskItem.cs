namespace Finstar.TaskManagement.Models.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public TaskStatus Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public Guid? UserId { get; set; }
    
    public User? User { get; set; }
}