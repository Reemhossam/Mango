using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mango.Services.AuthAPI.Service
{
    public class JwtTokenGenerator(IOptions<JWTOptions> jwtOption) : IJwtTokenGenerator
    {
        public string GenerateToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtOption.Value.Secret);
            var claimList = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
                new Claim(JwtRegisteredClaimNames.Name, applicationUser.UserName),
            };
            var tokenDescripter = new SecurityTokenDescriptor
            {
                Issuer = jwtOption.Value.Issuer,
                Audience = jwtOption.Value.Audience,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescripter);
            return tokenHandler.WriteToken(token);  //عشان يرجعها string مش object

        }
    }
}
