using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace jaswer2.Controllers;
[Route("/api/[controller]/{action}")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [HttpPost]
    public IActionResult GetData(){
        return Ok("Todo jalo al chiongazo");
    }

}

