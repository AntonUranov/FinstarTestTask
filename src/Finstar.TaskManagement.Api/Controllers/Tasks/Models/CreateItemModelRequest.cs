using System.ComponentModel.DataAnnotations;

namespace Finstar.TaskManagement.Api.Controllers.Tasks.Models;

public class CreateItemModelRequest
{
    [Required] 
    public string Title { get; init; } = null!;
    [Required]
    public string Description { get; init; } = null!;
    
    public Guid? UserId { get; init; }
}