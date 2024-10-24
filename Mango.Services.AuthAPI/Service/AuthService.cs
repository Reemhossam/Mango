using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService(AppDbContext _db, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager, JwtTokenGenerator _jwtTokenGenerator) : IAuthService
    {
        public async Task<bool> AssignRoleAsync(string email, string roleName)
        {
            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user is not null) 
            { 
                if(! await _roleManager.RoleExistsAsync(roleName))
                {
                    // create role as it is not exist
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
                    await _userManager.AddToRoleAsync(user, roleName);
                    return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(loginRequestDto.UserName);
            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if(user is null ||  !isPasswordValid) 
            { 
                return new LoginResponseDto() { User = null, Token = "" };
            }
            UserDto userDto = new ()
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
            };
            //Generate JWT token
            string token =_jwtTokenGenerator.GenerateToken(user);
            return new LoginResponseDto() { User = userDto, Token = token };
        }

        public async Task<string> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber,
                
            };
            try
            {
                var result = await _userManager.CreateAsync(user,registrationRequestDto.Password);
                if(result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == registrationRequestDto.Email);
                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        Name = userToReturn.Name,
                        Id = userToReturn.Id,
                        PhoneNumber = userToReturn.PhoneNumber
                    };
                    return "";
                }
                return result.Errors.FirstOrDefault().Description;
            }
            catch (Exception ex)
            {

            }
            return "Error Encountered";
        }
    }
}
