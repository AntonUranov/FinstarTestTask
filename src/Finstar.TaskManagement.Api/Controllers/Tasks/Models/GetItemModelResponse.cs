namespace Finstar.TaskManagement.Api.Controllers.Tasks.Models;

public class GetItemModelResponse: ApiResult
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init ; }
}