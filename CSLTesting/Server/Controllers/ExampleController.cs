using Microsoft.AspNetCore.Mvc;

namespace CSLTesting.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{

    [HttpGet]
    public Task<string> GetExample()
    {
        return Task.FromResult("Hello World");
    }

}