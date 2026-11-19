namespace Finstar.TaskManagement.Api.Handlers.GetTaskItem;

public class GetTaskItemResponse
{
    public required Guid TaskItemId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}