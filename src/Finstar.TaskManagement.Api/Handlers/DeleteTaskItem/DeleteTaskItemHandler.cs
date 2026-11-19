using Finstar.TaskManagement.ChangeTracker.Contracts;
using Finstar.TaskManagement.Dal;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Api.Handlers.DeleteTaskItem;

public class DeleteTaskItemHandler : IRequestHandler<DeleteTaskItemRequest, Result>
{
    private readonly ITaskManagementStorage _storage;
    private readonly IPublishEndpoint _publisher;

    public DeleteTaskItemHandler(ITaskManagementStorage storage, IPublishEndpoint publisher)
    {
        _storage = storage;
        _publisher = publisher;
    }
    public async Task<Result> Handle(DeleteTaskItemRequest request, CancellationToken cancellationToken)
    {
        var itemToDelete = await _storage.TaskItems.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (itemToDelete != null)
        {
            _storage.TaskItems.Remove(itemToDelete);
            await _publisher.Publish(itemToDelete.CreateTaskChangedEvent(TaskChangeType.Deleted), cancellationToken);
            await _storage.SaveChangesAsync(cancellationToken);
        }
        
        return Result.Success();
    }
}