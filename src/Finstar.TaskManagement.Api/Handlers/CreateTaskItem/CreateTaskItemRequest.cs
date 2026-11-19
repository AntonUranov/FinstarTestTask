using MediatR;

namespace Finstar.TaskManagement.Api.Handlers.CreateTaskItem;

public class CreateTaskItemRequest : IRequest<Result<CreateTaskItemResponse>>
{
    public string Description { get; init; }
    public string Title { get; init; }
    public Guid? UserId { get; set; }
}