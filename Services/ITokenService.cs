using EmlakPortal.API.Models;

namespace EmlakPortal.API.Services
{
    public interface ITokenService
    {
        Task<string> GenerateToken(AppUser user);
    }
}