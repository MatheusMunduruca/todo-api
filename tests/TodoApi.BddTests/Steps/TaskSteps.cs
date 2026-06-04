using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoApi.BddTests.Support;
using TodoApi.DTOs;
using TechTalk.SpecFlow;

namespace TodoApi.BddTests.Steps;

[Binding]
public class TaskSteps
{
    private readonly ScenarioState _state;

    public TaskSteps(ScenarioState state) => _state = state;

    [Given(@"que estou autenticado na taverna")]
    public async Task DadoAutenticado() => await _state.RegisterAndAuthenticateAsync();

    [Given(@"que nao estou autenticado")]
    public void DadoNaoAutenticado()
    {
        // O cliente padrão já não possui token — nada a fazer.
    }

    [Given(@"eu tenho uma missao chamada ""(.*)""")]
    public async Task DadoTenhoUmaMissao(string titulo) => await _state.CreateTaskAsync(titulo);

    [When(@"eu peco uma missao chamada ""(.*)""")]
    public async Task QuandoPecoUmaMissao(string titulo) => await _state.CreateTaskAsync(titulo);

    [When(@"eu concluo a missao")]
    public async Task QuandoConcluoAMissao()
    {
        _state.LastResponse = await _state.Client.PutAsJsonAsync(
            $"/api/tasks/{_state.LastTask!.Id}",
            new UpdateTaskRequest(null, null, "Done", null));
        _state.LastTask = await _state.LastResponse.Content.ReadFromJsonAsync<TaskResponse>();
    }

    [When(@"eu listo minhas missoes")]
    public async Task QuandoListoMinhasMissoes()
        => _state.LastResponse = await _state.Client.GetAsync("/api/tasks");

    [Then(@"a missao e criada com sucesso")]
    public void EntaoMissaoCriada()
        => _state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);

    [Then(@"a missao aparece com status ""(.*)""")]
    public void EntaoMissaoComStatus(string status)
        => _state.LastTask!.Status.Should().Be(status);

    [Then(@"a missao fica com status ""(.*)""")]
    public void EntaoMissaoFicaComStatus(string status)
    {
        _state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _state.LastTask!.Status.Should().Be(status);
    }

    [Then(@"o acesso e negado")]
    public void EntaoAcessoNegado()
        => _state.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
