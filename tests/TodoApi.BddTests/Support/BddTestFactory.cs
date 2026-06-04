using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoApi.Data;

namespace TodoApi.BddTests.Support;

/// <summary>Sobe a Todo API em memória para os cenários BDD (banco InMemory + config via env vars).</summary>
public class BddTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "bdd-" + Guid.NewGuid();

    // Configura JWT/conexão via variáveis de ambiente (lidas no CreateBuilder) —
    // independe do appsettings.json, então funciona também no CI.
    static BddTestFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", "chave-de-teste-bem-longa-hmacsha256-todoapi-bdd-0123456789");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TodoApi");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TodoApiUsers");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Server=localhost;Database=t;User=root;Password=t;");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(AppDbContext)
                         || d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(_dbName));
        });
    }
}
