using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mango.Services.CouponAPI.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAuthenticationAndAuthorization(this WebApplicationBuilder builder)
        {
            var SettingsSection = builder.Configuration.GetSection("ApiSettings");
            var Issuer = SettingsSection.GetValue<string>("Issuer");
            var Audience = SettingsSection.GetValue<string>("Audience");
            var Secret = SettingsSection.GetValue<string>("Secret");
            var key = Encoding.ASCII.GetBytes(Secret);
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
            builder.Services.AddAuthorization();
            
            return builder;
        }
    }
}
