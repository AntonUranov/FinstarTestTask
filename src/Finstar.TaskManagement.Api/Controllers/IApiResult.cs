namespace Finstar.TaskManagement.Api.Controllers;

public abstract class ApiResult
{
    public bool Ok { get; init; }
    public string Code { get; init; }
    public string Message { get; init; }
}