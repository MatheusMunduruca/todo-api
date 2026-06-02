using System.Net;
using System.Net.Http.Json;
using TodoApi.DTOs;
using TodoApi.IntegrationTests.Fixtures;
using TodoApi.IntegrationTests.Helpers;

namespace TodoApi.IntegrationTests.Auth;

public class AuthIntegrationTests : IDisposable
{
    private readonly TodoApiFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests()
    {
        _factory = new TodoApiFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ComDadosValidos_Retorna200ComToken()
    {
        var request = DataGenerator.NewUser();

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data.Should().NotBeNull();
        data!.Token.Should().NotBeEmpty();
        data.Name.Should().Be(request.Name);
        data.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_Retorna409()
    {
        var request = DataGenerator.NewUser();

        await _client.PostAsJsonAsync("/api/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ComEmailInvalido_Retorna400()
    {
        var request = new RegisterRequest("Matheus", "email-invalido", "senha123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_TokenGerado_EhUmJwtValido()
    {
        var request = DataGenerator.NewUser();

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // JWT tem 3 partes separadas por ponto
        data!.Token.Split('.').Should().HaveCount(3);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200ComToken()
    {
        var user = DataGenerator.NewUser();
        await _client.PostAsJsonAsync("/api/auth/register", user);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            DataGenerator.LoginFrom(user));
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        data!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Retorna401()
    {
        var user = DataGenerator.NewUser();
        await _client.PostAsJsonAsync("/api/auth/register", user);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(user.Email, "senhaErrada123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("naoexiste@email.com", "senha123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
