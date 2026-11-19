namespace Finstar.TaskManagement.ChangeTracker.Contracts;

public sealed class TaskChangedEvent
{
    public Guid EventId { get; init; }
    public Guid TaskId { get; init; } 
    public TaskChangeType ChangeType { get; init; } 
    public string Title { get; init; } 
    public string Description { get; init; } 
    public DateTimeOffset UpdatedAt { get; init; } 
}