using System.Net;
using System.Net.Http.Json;
using TodoApi.DTOs;
using TodoApi.IntegrationTests.Fixtures;
using TodoApi.IntegrationTests.Helpers;

namespace TodoApi.IntegrationTests.Tasks;

public class TasksIntegrationTests : IDisposable
{
    private readonly TodoApiFactory _factory;

    public TasksIntegrationTests()
    {
        _factory = new TodoApiFactory();
    }

    public void Dispose() => _factory.Dispose();

    // ── Autenticação ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_SemToken_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_SemToken_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Listagem ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasks_UsuarioSemTarefas_RetornaListaVazia()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var response = await client.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasks_RetornaApenasAsTarefasDoUsuarioAutenticado()
    {
        var (token1, _, _) = await _factory.RegisterUserAsync();
        var (token2, _, _) = await _factory.RegisterUserAsync();

        var client1 = _factory.CreateAuthenticatedClient(token1);
        var client2 = _factory.CreateAuthenticatedClient(token2);

        // Usuário 1 cria 2 tarefas, usuário 2 cria 1
        await client1.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        await client1.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        await client2.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());

        var response = await client1.GetAsync("/api/tasks");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();

        tasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTasks_FiltrandoPorStatus_RetornaApenasAsCorrespondentes()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        await client.PutAsJsonAsync($"/api/tasks/{created!.Id}", DataGenerator.UpdateWith("InProgress"));

        await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());

        var response = await client.GetAsync("/api/tasks?status=InProgress");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();

        tasks.Should().HaveCount(1);
        tasks!.First().Status.Should().Be("InProgress");
    }

    // ── Criar tarefa ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_ComDadosValidos_Retorna201ComTarefa()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);
        var request = DataGenerator.NewTask();

        var response = await client.PostAsJsonAsync("/api/tasks", request);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        task!.Title.Should().Be(request.Title);
        task.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task CreateTask_SemDescricao_Retorna201()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync("/api/tasks",
            DataGenerator.NewTaskWithoutDescription());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ── Buscar por ID ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTaskById_TarefaExistente_Retorna200()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.GetAsync($"/api/tasks/{created!.Id}");
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        task!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetTaskById_TarefaInexistente_Retorna404()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var response = await client.GetAsync("/api/tasks/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTaskById_TarefaDeOutroUsuario_Retorna404()
    {
        var (token1, _, _) = await _factory.RegisterUserAsync();
        var (token2, _, _) = await _factory.RegisterUserAsync();

        var client1 = _factory.CreateAuthenticatedClient(token1);
        var client2 = _factory.CreateAuthenticatedClient(token2);

        var createResponse = await client1.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        // Usuário 2 tenta acessar tarefa do usuário 1
        var response = await client2.GetAsync($"/api/tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Atualizar ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_AlterandoStatus_Retorna200ComNovoStatus()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{created!.Id}",
            DataGenerator.UpdateWith("InProgress"));
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task UpdateTask_TarefaInexistente_Retorna404()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var response = await client.PutAsJsonAsync(
            "/api/tasks/99999",
            DataGenerator.UpdateWith("Done"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Deletar ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTask_TarefaExistente_Retorna204()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        var response = await client.DeleteAsync($"/api/tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_TarefaExistente_RemoveDaLista()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var createResponse = await client.PostAsJsonAsync("/api/tasks", DataGenerator.NewTask());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();

        await client.DeleteAsync($"/api/tasks/{created!.Id}");

        var listResponse = await client.GetAsync("/api/tasks");
        var tasks = await listResponse.Content.ReadFromJsonAsync<List<TaskResponse>>();
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTask_TarefaInexistente_Retorna404()
    {
        var (token, _, _) = await _factory.RegisterUserAsync();
        var client = _factory.CreateAuthenticatedClient(token);

        var response = await client.DeleteAsync("/api/tasks/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
