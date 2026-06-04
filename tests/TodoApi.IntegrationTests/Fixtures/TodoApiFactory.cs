using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;
using TodoApi.DTOs;

namespace TodoApi.IntegrationTests.Fixtures;

public class TodoApiFactory : WebApplicationFactory<Program>
{
    // Configura JWT/conexão via variáveis de ambiente — lidas já no CreateBuilder,
    // então valem tanto para a geração quanto para a validação do token.
    // Torna os testes independentes do appsettings.json (que é ignorado no Git → CI).
    static TodoApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "chave-de-teste-bem-longa-hmacsha256-todoapi-0123456789");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TodoApi");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TodoApiUsers");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Server=localhost;Database=t;User=root;Password=t;");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Gera o nome do banco UMA vez por factory — garante isolamento entre testes
        // e persistência dos dados dentro do mesmo teste
        var dbName = Guid.NewGuid().ToString();

        builder.ConfigureTestServices(services =>
        {
            // Remove registros do DbContext com MySQL
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

            foreach (var d in toRemove) services.Remove(d);

            // Substitui por banco em memória com nome fixo para esta factory
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(dbName));
        });
    }

    /// <summary>Cria um HttpClient autenticado com o token informado.</summary>
    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Registra um novo usuário e retorna (token, email, senha).</summary>
    public async Task<(string Token, string Email, string Password)> RegisterUserAsync()
    {
        var client = CreateClient();
        var faker = new Bogus.Faker("pt_BR");

        var request = new RegisterRequest(
            faker.Name.FullName(),
            faker.Internet.Email(),
            faker.Internet.Password(12)
        );

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();

        return (data!.Token, request.Email, request.Password);
    }
}
