using System.ComponentModel.DataAnnotations;

namespace Finstar.TaskManagement.Api.Options;

public class DbExecutionStrategyOptions
{
    [Range(1, int.MaxValue)]
    public int MaxRetryCount { get; init; }
    
    [Range(1, int.MaxValue)]
    public int MaxDelaySeconds { get; set; }
}