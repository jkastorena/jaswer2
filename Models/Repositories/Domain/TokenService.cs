using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace jaswer2;

public class TokenService : ITokenService
{

    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;   
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string Token)
    {
        var TokenValidationParameters = new TokenValidationParameters{
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Token)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(Token, TokenValidationParameters, out securityToken);
        var JwtSecurityToken = securityToken as JwtSecurityToken;
        if(JwtSecurityToken == null || !JwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)){
            throw new SecurityTokenException("Invalid Token");
        }

        return principal;
    }

    public string GetRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng =  RandomNumberGenerator.Create()){
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        throw new NotImplementedException();
    }

    public TokenResponse GetToken(IEnumerable<Claim> claim)
    {
        var authSigningKey =  new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])) ;
        var token = new JwtSecurityToken(
            issuer : _configuration["JWT:ValidIssuer"],
            audience : _configuration["JWT:ValidAudience"],
            expires : DateTime.Now.AddDays(7),
            claims : claim,
            signingCredentials : new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse {TokenString = tokenString, ValidTo = token.ValidTo};
    }
}
