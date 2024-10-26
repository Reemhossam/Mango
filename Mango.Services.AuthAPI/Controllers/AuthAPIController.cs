using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Mango.Services.AuthPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController(IAuthService _authService) : ControllerBase
    {
        protected ResponseDto _response= new();
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationRequestDto model)
        {
            string errorMessage= await _authService.RegisterAsync(model);
            if(!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            var loginResponse = await _authService.LoginAsync(model);
            if(loginResponse.User is null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or Password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }
        [HttpPost("assignRole")]
        public async Task<IActionResult> AssignRole(RegistrationRequestDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRoleAsync(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            return Ok(_response);
        }
    }
}
