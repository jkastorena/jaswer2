using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using jaswer2;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace jaswer2.Controllers;

[Route("api/[controller]/{action}")]
[ApiController]
public class TokenController : Controller
{
    private readonly DatabaseContext _ctx;
    private readonly ITokenService _service;
    public TokenController(DatabaseContext ctx, ITokenService service)
    {
        _ctx = ctx;
        _service = service;
    }

    [HttpPost]
    public IActionResult Refresh(RefreshtokenRequest tokenApiModel){
        if(tokenApiModel == null){
            return BadRequest("Invalid Client Request");
        }
        string accessToken = tokenApiModel.AccessToken;
        string refreshToken = tokenApiModel.RefreshToken;
        var principal = _service.GetPrincipalFromExpiredToken(accessToken);
        var username = principal.Identity.Name;
        var user = _ctx.TokenInfo.SingleOrDefault(u => u.Username == username);
        if(user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.Now){
            return BadRequest("Invalid Client Request");
        }
        var newAccessToken = _service.GetToken(principal.Claims);
        var newRefreshToken = _service.GetRefreshToken();
        user.RefreshToken = newRefreshToken;
        _ctx.SaveChanges();
        return Ok(new RefreshtokenRequest()
        {
            AccessToken = newAccessToken.TokenString,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost, Authorize]
    public IActionResult Revoke(){
        try{
                        var username = User.Identity.Name;
        var user = _ctx.TokenInfo.SingleOrDefault(u => u.Username == username);
        if (user == null ) return BadRequest();
        user.RefreshToken = null;
        _ctx.SaveChanges();
        return Ok(true);
        }
        catch(Exception ex){
            return BadRequest(ex.Message);
        }
    }

}
