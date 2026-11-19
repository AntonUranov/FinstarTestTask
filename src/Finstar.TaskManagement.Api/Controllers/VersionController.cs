using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Finstar.TaskManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VersionController : ControllerBase
{
    private static readonly AssemblyName AssemblyName = Assembly.GetExecutingAssembly().GetName();

    [HttpGet]
    public IActionResult GetVersion() => Content($"{AssemblyName.Name ?? ""} {AssemblyName.Version?.ToString() ?? ""}", "plain/text");
}