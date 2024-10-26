namespace Mango.Web.Service.IService
{
    public interface ITokenProvidor
    {
        void SetToken(string token);
        string? GetToken();
        void ClearToken();
    }
}
