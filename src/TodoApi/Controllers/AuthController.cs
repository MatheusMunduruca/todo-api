using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const decimal StartingGold = 1000m;
    private const string GregorWelcome = "Aqui está um trocadinho para começar, bem vindo a Taverna!";

    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Registra um novo usuário e concede 1000 Gold Coins de boas-vindas.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "E-mail já cadastrado." });

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            GoldBalance = StartingGold
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Name, user.Email, user.GoldBalance, GregorWelcome));
    }

    /// <summary>
    /// Autentica um usuário existente e retorna JWT + saldo de Gold.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciais inválidas." });

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Name, user.Email, user.GoldBalance));
    }

    /// <summary>
    /// Retorna o saldo de Gold do usuário autenticado.
    /// </summary>
    [HttpGet("gold")]
    [Authorize]
    public async Task<IActionResult> GetGold()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();
        return Ok(new { gold = user.GoldBalance });
    }

    /// <summary>
    /// Deduz Gold do usuário autenticado (usado pelo ecommerce-api via chamada interna).
    /// </summary>
    [HttpPost("gold/deduct")]
    [Authorize]
    public async Task<IActionResult> DeductGold([FromBody] DeductGoldRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (user.GoldBalance < req.Amount)
            return BadRequest(new { message = "Gold insuficiente.", current = user.GoldBalance });

        user.GoldBalance -= req.Amount;
        await _db.SaveChangesAsync();

        return Ok(new { gold = user.GoldBalance });
    }

    /// <summary>
    /// Adiciona Gold ao usuário autenticado.
    /// </summary>
    [HttpPost("gold/add")]
    [Authorize]
    public async Task<IActionResult> AddGold([FromBody] DeductGoldRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        user.GoldBalance += req.Amount;
        await _db.SaveChangesAsync();

        return Ok(new { gold = user.GoldBalance });
    }
}
