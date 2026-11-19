using Finstar.TaskManagement.Api.Controllers.Tasks.Models;
using Finstar.TaskManagement.Api.Handlers;
using Finstar.TaskManagement.Api.Handlers.CreateTaskItem;
using Finstar.TaskManagement.Api.Handlers.GetTaskItem;

namespace Finstar.TaskManagement.Api.Controllers.Tasks;

public interface ITaskItemsMapper
{
    CreateItemModelResponse ToApiResponse(CreateTaskItemResponse response);
    GetItemModelResponse ToApiResponse(GetTaskItemResponse response);
    ProblemDetailsResponse ToApiProblem(Result failedResult);
}

public class TaskItemsMapper : ITaskItemsMapper
{
    public CreateItemModelResponse ToApiResponse(CreateTaskItemResponse response) =>
        new()
        {
            Id = response.Id,
            Ok = true
        };

    public ProblemDetailsResponse ToApiProblem(Result failedResult) =>
        new()
        {
            Ok = false,
            Code = failedResult.Error.Code,
            Message = failedResult.Error.Description
        };
    
    public GetItemModelResponse ToApiResponse(GetTaskItemResponse response) =>
        new()
        {
            Id = response.TaskItemId,
            Title = response.Title,
            Description = response.Description,
            Ok = true
        };
    
}