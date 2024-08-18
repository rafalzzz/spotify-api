using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SpotifyApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires);
        SigningCredentials GetSigningCredentials(string secretKey);
    }

    public class JwtService() : IJwtService
    {

        public SigningCredentials GetSigningCredentials(string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string GenerateToken(List<Claim> claims, string issuer, string audience, string secretKey, DateTime expires)
        {
            var creds = GetSigningCredentials(secretKey);

            JwtSecurityToken token = new(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}