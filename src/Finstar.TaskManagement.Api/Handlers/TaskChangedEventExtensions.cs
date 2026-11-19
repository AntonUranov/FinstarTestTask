using Finstar.TaskManagement.ChangeTracker.Contracts;
using Finstar.TaskManagement.Models.Entities;

namespace Finstar.TaskManagement.Api.Handlers;

public static class TaskChangedEventExtensions
{
    public static TaskChangedEvent CreateTaskChangedEvent(this TaskItem item, TaskChangeType  changeType) =>
        new()
        {
            EventId = Guid.NewGuid(),
            TaskId = item.Id,
            ChangeType = changeType,
            Title = item.Title,
            Description = item.Description,
            UpdatedAt = item.UpdatedAt
        };
}