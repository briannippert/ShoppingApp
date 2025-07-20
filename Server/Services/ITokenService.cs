using Server.Models;

namespace Server.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
