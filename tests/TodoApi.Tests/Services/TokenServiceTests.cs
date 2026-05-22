using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Services;

public class TokenServiceTests
{
    private static TokenService CreateService()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-with-at-least-32-chars!");
        config.Setup(c => c["Jwt:Issuer"]).Returns("TodoApi");
        config.Setup(c => c["Jwt:Audience"]).Returns("TodoApiUsers");
        return new TokenService(config.Object);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var service = CreateService();
        var user = new User { Id = 1, Name = "Matheus", Email = "matheus@email.com" };

        var token = service.GenerateToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        var service = CreateService();
        var user = new User { Id = 42, Name = "Matheus", Email = "matheus@email.com" };

        var token = service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("42", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("matheus@email.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("Matheus", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void GenerateToken_ExpiresInApproximately8Hours()
    {
        var service = CreateService();
        var user = new User { Id = 1, Name = "Matheus", Email = "matheus@email.com" };

        var before = DateTime.UtcNow.AddHours(7.9);
        var token = service.GenerateToken(user);
        var after = DateTime.UtcNow.AddHours(8.1);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.InRange(jwt.ValidTo, before, after);
    }
}
