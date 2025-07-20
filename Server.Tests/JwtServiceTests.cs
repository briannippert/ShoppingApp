using Server.Models;
using Server.Services;
using Xunit;

namespace Server.Tests;

public class JwtServiceTests
{
    [Fact]
    public void GenerateJwtToken_ReturnsTokenString()
    {
        var user = new User { Id = 1, FullName = "Test User", Email = "test@example.com" };
        var secret = "thisisaveryverylongsecretkey123456";

        var token = JwtService.GenerateJwtToken(user, secret);

        Assert.False(string.IsNullOrEmpty(token));
    }
}
