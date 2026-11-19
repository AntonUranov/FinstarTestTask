using Finstar.TaskManagement.ChangeTracker.Dal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finstar.TaskManagement.ChangeTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskChangesController : ControllerBase
{
    private readonly ITaskChangesStorage _storage;

    public TaskChangesController(ITaskChangesStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var items = await _storage.TaskChanges
            .Where(x=> x.TaskId == id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        
        return Ok(items);
    }
}