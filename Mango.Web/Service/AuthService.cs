using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class AuthService(IBaseService _baseService) : IAuthService
    {
        public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.AuthAPIBase + "/api/auth/assignRole",
                ApiType = SD.ApiType.POST,
                Data = registrationRequestDto,
            }, withBearer:false);
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            return await _baseService.SendAsync(new()
            {
                Url= SD.AuthAPIBase +"/api/auth/login",
                ApiType = SD.ApiType.POST,
                Data = loginRequestDto,
            }, withBearer: false);
        }

        public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            return await _baseService.SendAsync(new()
            {
                Url = SD.AuthAPIBase + "/api/auth/register",
                ApiType = SD.ApiType.POST,
                Data = registrationRequestDto,
            }, withBearer: false);
        }
    }
}
