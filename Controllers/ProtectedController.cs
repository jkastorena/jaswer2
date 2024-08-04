using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace jaswer2.Controllers;

[Route("api/[controller]/{action}")]
[ApiController]
[Authorize]
public class ProtectedController : ControllerBase
{
    [HttpPost]
    public IActionResult GetData(){
        return Ok("Todo esta finolis");
    }
}

