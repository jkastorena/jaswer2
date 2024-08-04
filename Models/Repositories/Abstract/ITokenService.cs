using System.Security.Claims;

namespace jaswer2;

public interface ITokenService
{
    TokenResponse GetToken(IEnumerable<Claim> claim);
    string GetRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string Token);
}
