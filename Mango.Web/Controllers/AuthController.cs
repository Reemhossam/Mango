using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController(IAuthService _authService, ITokenProvidor _tokenProvidor) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem(){Text = SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem(){Text = SD.RoleCustomer, Value=SD.RoleCustomer}
            };
            ViewBag.RoleList = roleList;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvidor.ClearToken();
            return RedirectToAction("Index","Home");
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto model)
        {
            if (ModelState.IsValid)
            { 
                ResponseDto result = await _authService.RegisterAsync(model);
                if (result !=null && result.IsSuccess)
                {
                    if (string.IsNullOrEmpty(model.Role))
                    {
                        model.Role = SD.RoleCustomer;
                    }
                    ResponseDto assignRole = await _authService.AssignRoleAsync(model);
                    if (assignRole != null && assignRole.IsSuccess) 
                    {
                        TempData["success"] = "Registeration Successful";
                        return RedirectToAction(nameof(Login));
                    } 
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem(){Text = SD.RoleAdmin, Value=SD.RoleAdmin},
                new SelectListItem(){Text = SD.RoleCustomer, Value=SD.RoleCustomer}
            };
            ViewBag.RoleList = roleList;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            ResponseDto responseDto = await _authService.LoginAsync(model);
            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDto loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));
                SignInUser(loginResponseDto);
                _tokenProvidor.SetToken(loginResponseDto.Token);
                return RedirectToAction("Index","Home");
            }
            else 
            {
                ModelState.AddModelError("CustomError", responseDto.Message);
                TempData["Error"] = responseDto.Message;
                return View(model);
            }
            
        }
        private async Task SignInUser(LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(model.Token);
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,       // name here meaning UserName 
                jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role,       
                jwt.Claims.FirstOrDefault(c => c.Type == "role").Value));

            var principle = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principle);
        }
    }
}
