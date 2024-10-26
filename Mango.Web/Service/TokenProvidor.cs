using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Mango.Web.Service
{
    public class TokenProvidor(IHttpContextAccessor _contextAccessor) : ITokenProvidor
    {
        public void ClearToken()
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string token=null;
            bool hasToken= _contextAccessor.HttpContext.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return hasToken? token :null;
        }

        public void SetToken(string token)
        {
            _contextAccessor.HttpContext.Response.Cookies.Append(SD.TokenCookie, token);
        }
    }
}
