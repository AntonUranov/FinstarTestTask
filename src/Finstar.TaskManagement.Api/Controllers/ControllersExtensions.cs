using Microsoft.AspNetCore.Mvc;

namespace Finstar.TaskManagement.Api.Controllers;

public static class ControllersExtensions
{
    public static ObjectResult ProblemDetailsResult(this ApiResult apiResult)
    {
        return new ObjectResult(apiResult)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ContentTypes = { "application/json" }
        };
    }
}