using Asp.Versioning;
using Finstar.TaskManagement.Api.Controllers.Tasks.Models;
using Finstar.TaskManagement.Api.Handlers;
using Finstar.TaskManagement.Api.Handlers.CreateTaskItem;
using Finstar.TaskManagement.Api.Handlers.DeleteTaskItem;
using Finstar.TaskManagement.Api.Handlers.GetTaskItem;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Finstar.TaskManagement.Api.Controllers.Tasks;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class TaskController: ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITaskItemsMapper _mapper;

    public TaskController(IMediator mediator, ITaskItemsMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute]Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTaskItemRequest
        {
            TaskItemId = id
        }, cancellationToken);

        if (result.IsFailure)
        {
            var problem = _mapper.ToApiProblem(result);
            
            return result.Error.Code switch
            {
                ErrorCodes.GetTaskItem.ItemNotFound => NotFound(problem),
                
                _ => problem.ProblemDetailsResult()
            };
        }

        return Ok(_mapper.ToApiResponse(result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody]CreateItemModelRequest modelRequest, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateTaskItemRequest
        {
            Title = modelRequest.Title,
            Description = modelRequest.Description,
            UserId = modelRequest.UserId,
        }, cancellationToken);

        if (result.IsFailure)
        {
            var problem = _mapper.ToApiProblem(result);
            
            return result.Error.Code switch
            {
                ErrorCodes.CreateTaskItem.UserNotFound => UnprocessableEntity(problem),
                
                _ => problem.ProblemDetailsResult()
            };
        }
        
        return Ok(_mapper.ToApiResponse(result.Data!));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteTaskItemRequest
        {
            Id = id
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            var problem = _mapper.ToApiProblem(result);
            return problem.ProblemDetailsResult();
        }

        return NoContent();
    }
}