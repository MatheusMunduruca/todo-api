using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests.Controllers;

public class AuthControllerTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static TokenService CreateTokenService()
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-with-at-least-32-chars!");
        config.Setup(c => c["Jwt:Issuer"]).Returns("TodoApi");
        config.Setup(c => c["Jwt:Audience"]).Returns("TodoApiUsers");
        return new TokenService(config.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        using var db = CreateDb();
        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Register(new RegisterRequest("Matheus", "matheus@email.com", "senha123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.NotEmpty(response.Token);
        Assert.Equal("Matheus", response.Name);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        using var db = CreateDb();
        db.Users.Add(new User { Name = "Outro", Email = "matheus@email.com", PasswordHash = "hash" });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Register(new RegisterRequest("Matheus", "matheus@email.com", "senha123"));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        using var db = CreateDb();
        db.Users.Add(new User
        {
            Name = "Matheus",
            Email = "matheus@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123")
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Login(new LoginRequest("matheus@email.com", "senha123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.NotEmpty(response.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var db = CreateDb();
        db.Users.Add(new User
        {
            Name = "Matheus",
            Email = "matheus@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("senha123")
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Login(new LoginRequest("matheus@email.com", "senhaErrada"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        using var db = CreateDb();
        var controller = new AuthController(db, CreateTokenService());

        var result = await controller.Login(new LoginRequest("naoexiste@email.com", "senha123"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
