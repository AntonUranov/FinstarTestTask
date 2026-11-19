using Finstar.TaskManagement.ChangeTracker.Contracts;
using Finstar.TaskManagement.ChangeTracker.Dal;
using MassTransit;

namespace Finstar.TaskManagement.ChangeTracker;

public class TaskChangedEventConsumer: IConsumer<TaskChangedEvent>
{
    private readonly ITaskChangesStorage _storage;
    private readonly ILogger<TaskChangedEventConsumer> _logger;

    public TaskChangedEventConsumer(ITaskChangesStorage storage, ILogger<TaskChangedEventConsumer> logger)
    {
        _storage = storage;
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<TaskChangedEvent> context)
    {
        var message = context.Message;
        if (_storage.TaskChanges.Any(x => x.EventId == message.EventId))
        {
            _logger.LogInformation("Duplicated message received: {EventId}", message.EventId);
            return;
        }

        var itemToSave = new TaskChange
        {
            EventId = message.EventId,
            TaskId = message.TaskId,
            ChangeType = message.ChangeType.ToString(),
            Description = message.Description,
            Title = message.Title,
            UpdatedAt = message.UpdatedAt
        };

        _storage.TaskChanges.Add(itemToSave);
        
        await _storage.SaveChangesAsync();
    }
}