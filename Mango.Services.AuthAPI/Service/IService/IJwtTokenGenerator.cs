using Mango.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser applicationUseruser);
    }
}
