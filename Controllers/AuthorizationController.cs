using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Identity;
using jaswer2;
using jaswer2.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace jaswer2.Controllers;

[Route("api/[controller]/{Action}")]
[ApiController]
public class AuthorizationController : Controller {

    private readonly DatabaseContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthorizationController(DatabaseContext context,
                                   UserManager<ApplicationUser> userManager,
                                   RoleManager<IdentityRole> roleManager,
                                   ITokenService tokenService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {

        var status = new Status();
        if (!ModelState.IsValid)
        {
            status.StatusCode = 0;
            status.Message = "Not all fields are filled";
            return Ok(status);
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            status.StatusCode = 0;
            status.Message = "Invalid Username";
            return Ok(status);
        }

        if (await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
        {
            status.StatusCode = 0;
            status.Message = "Invalid current password";
            return Ok(status);
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            status.StatusCode = 0;
            status.Message = "Failed to change password";
            return Ok(status);
        }
        status.StatusCode = 1;
        status.Message = "Password has changed successfully";
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RegistrationAdmin([FromBody] RegistrationModel model)
    {

        var status = new Status();
        if (!ModelState.IsValid)
        {
            status.StatusCode = 0;
            status.Message = "Please pass all the required fields";
            return Ok(status);
        }

        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            status.StatusCode = 0;
            status.Message = "Invalid Username";
            return Ok(status);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            SecurityStamp = Guid.NewGuid().ToString(),
            Email = model.Email,
            Name = model.Name
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            status.StatusCode = 0;
            status.Message = "User creation failed";
            return Ok(status);
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        }

        if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.Admin);
        }
        status.StatusCode = 1;
        status.Message = "Successfully registered";
        return Ok(status);
    }
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);

        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim> {
                 new Claim(ClaimTypes.Name, user.UserName),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())};
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }
            var token = _tokenService.GetToken(authClaims);
            var refreshToken = _tokenService.GetRefreshToken();
            var tokenInfo = _context.TokenInfo.FirstOrDefault(q => q.Username == user.UserName);
            if (tokenInfo == null)
            {
                var info = new TokenInfo
                {
                    Username = user.UserName,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = DateTime.Now.AddDays(7)
                };
            }
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            }
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new LoginResponse
            {
                Name = user.Name,
                Username = user.UserName,
                Token = token.TokenString,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo,
                StatusCode = 1,
                Message = "Loged in"
            });
        }
        return Ok(
            new LoginResponse
            {
                StatusCode = 0,
                Message = "Invalid Username or password",
                Token = "",
                Expiration = null
            }
        );
    }
    [HttpPost]
    public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
    {

        var status = new Status();
        if (!ModelState.IsValid)
        {
            status.StatusCode = 0;
            status.Message = "Please pass all the required fields";
            return Ok(status);
        }

        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            status.StatusCode = 0;
            status.Message = "Invalid Username";
            return Ok(status);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username,
            SecurityStamp = Guid.NewGuid().ToString(),
            Email = model.Email,
            Name = model.Name
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            status.StatusCode = 0;
            status.Message = "User creation failed";
            return Ok(status);
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        }

        if (await _roleManager.RoleExistsAsync(UserRoles.User))
        {
            await _userManager.AddToRoleAsync(user, UserRoles.User);
        }
        status.StatusCode = 1;
        status.Message = "Successfully registered";
        return Ok(status);
    }

}
