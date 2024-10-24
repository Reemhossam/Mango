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
    public class JwtTokenGenerator(JWTOptions jwtOption) : IJwtTokenGenerator
    {
        public string GenerateToken(ApplicationUser applicationUseruser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtOption.Secret);
            var claimList = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Email, applicationUseruser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUseruser.Id),
                new Claim(JwtRegisteredClaimNames.Name, applicationUseruser.UserName),
            };
            var tokenDescripter = new SecurityTokenDescriptor
            {
                Issuer = jwtOption.Issuer,
                Audience = jwtOption.Audience,
                Subject = new ClaimsIdentity(claimList),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescripter);
            return tokenHandler.WriteToken(token);  //عشان يرجعها string مش object

        }
    }
}
