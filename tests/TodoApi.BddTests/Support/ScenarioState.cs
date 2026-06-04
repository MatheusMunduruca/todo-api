using System.Net.Http.Headers;
using System.Net.Http.Json;
using TodoApi.DTOs;

namespace TodoApi.BddTests.Support;

/// <summary>
/// Estado compartilhado entre os passos de um cenário (um por cenário, injetado pelo SpecFlow).
/// </summary>
public class ScenarioState : IDisposable
{
    public BddTestFactory Factory { get; } = new();
    public HttpClient Client { get; }
    public HttpResponseMessage? LastResponse { get; set; }
    public TaskResponse? LastTask { get; set; }

    public ScenarioState() => Client = Factory.CreateClient();

    public async Task RegisterAndAuthenticateAsync()
    {
        var faker = new Bogus.Faker("pt_BR");
        var req = new RegisterRequest(faker.Name.FullName(), faker.Internet.Email(), "Senha123!");

        var resp = await Client.PostAsJsonAsync("/api/auth/register", req);
        resp.EnsureSuccessStatusCode();

        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
    }

    public async Task<TaskResponse> CreateTaskAsync(string title)
    {
        var resp = await Client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest(title, null, null));
        LastResponse = resp;
        resp.EnsureSuccessStatusCode();
        LastTask = await resp.Content.ReadFromJsonAsync<TaskResponse>();
        return LastTask!;
    }

    public void Dispose()
    {
        LastResponse?.Dispose();
        Client.Dispose();
        Factory.Dispose();
    }
}
