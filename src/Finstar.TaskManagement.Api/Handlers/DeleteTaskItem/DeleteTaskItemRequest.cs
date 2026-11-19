using MediatR;

namespace Finstar.TaskManagement.Api.Handlers.DeleteTaskItem;

public class DeleteTaskItemRequest : IRequest<Result>
{
    public Guid Id { get; init; }
}