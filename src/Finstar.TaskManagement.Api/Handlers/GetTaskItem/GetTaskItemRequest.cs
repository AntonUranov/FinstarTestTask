using MediatR;

namespace Finstar.TaskManagement.Api.Handlers.GetTaskItem;

public class GetTaskItemRequest: IRequest<Result<GetTaskItemResponse>>
{
    public Guid TaskItemId { get; init; }
}