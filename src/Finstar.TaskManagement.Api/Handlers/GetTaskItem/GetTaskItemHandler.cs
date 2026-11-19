using Finstar.TaskManagement.Dal;
using Finstar.TaskManagement.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.Api.Handlers.GetTaskItem;

public class GetTaskItemHandler(ITaskManagementStorage storage)
    : IRequestHandler<GetTaskItemRequest, Result<GetTaskItemResponse>>
{
    public async Task<Result<GetTaskItemResponse>> Handle(GetTaskItemRequest request, CancellationToken cancellationToken)
    {
        var item = await storage.TaskItems
            .Where(taskItem => taskItem.Id == request.TaskItemId)
            .ToGetTaskItemResponse()
            .AsNoTracking()
            .FirstOrDefaultAsync( cancellationToken: cancellationToken);
        
        if (item is null)
        {
            return Result.Failure<GetTaskItemResponse>(new Error(ErrorCodes.GetTaskItem.ItemNotFound,
                "The specified item does not exist in the database"));
        }

        return Result.Success(item);
    }
}

file static class ProjectionExtensions
{
    public static IQueryable<GetTaskItemResponse> ToGetTaskItemResponse(this IQueryable<TaskItem> source) =>
        source.
            Select(x => new GetTaskItemResponse
            {
                TaskItemId = x.Id,
                Description = x.Description,
                Title = x.Title
            });
}