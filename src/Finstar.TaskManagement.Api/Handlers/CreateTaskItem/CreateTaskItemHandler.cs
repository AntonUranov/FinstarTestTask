using Finstar.TaskManagement.ChangeTracker.Contracts;
using Finstar.TaskManagement.Dal;
using Finstar.TaskManagement.Models.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Finstar.TaskManagement.Models.TaskStatus;

namespace Finstar.TaskManagement.Api.Handlers.CreateTaskItem;

public class CreateTaskItemHandler : IRequestHandler<CreateTaskItemRequest, Result<CreateTaskItemResponse>>
{
    private readonly ITaskManagementStorage _storage;
    private readonly TimeProvider _timeProvider;
    private readonly IPublishEndpoint _publisher;

    public CreateTaskItemHandler(ITaskManagementStorage storage, TimeProvider timeProvider, IPublishEndpoint publisher)
    {
        _storage = storage;
        _timeProvider = timeProvider;
        _publisher = publisher;
    }

    public async Task<Result<CreateTaskItemResponse>> Handle(CreateTaskItemRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId != null)
        {
            var assignee = await _storage.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
            if (assignee is null)
            {
                return Result.Failure<CreateTaskItemResponse>(new Error(ErrorCodes.CreateTaskItem.UserNotFound,
                    "The specified user does not exist in the database"));
            }
        }
        
        var now = _timeProvider.GetUtcNow();
        var item = new TaskItem
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            Description = request.Description,
            Status = TaskStatus.New,
            UpdatedAt = now,
            CreatedAt = now,
            UserId = request.UserId
        };
        
        _storage.TaskItems.Add(item);

        await _publisher.Publish(item.CreateTaskChangedEvent(TaskChangeType.Created), cancellationToken);
        
        await _storage.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new CreateTaskItemResponse { Id =  item.Id });
    }

    
}