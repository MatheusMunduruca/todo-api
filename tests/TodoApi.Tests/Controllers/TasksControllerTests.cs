using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Tests.Controllers;

public class TasksControllerTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static TasksController CreateController(AppDbContext db, int userId)
    {
        var controller = new TasksController(db);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
        return controller;
    }

    // ── GET /api/tasks ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOnlyTasksOfAuthenticatedUser()
    {
        using var db = CreateDb();
        db.Tasks.AddRange(
            new TodoTask { Title = "Tarefa do usuário 1", UserId = 1 },
            new TodoTask { Title = "Tarefa do usuário 2", UserId = 2 }
        );
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).GetAll(null);

        var ok = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponse>>(ok.Value);
        Assert.Single(tasks);
        Assert.Equal("Tarefa do usuário 1", tasks.First().Title);
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsFilteredTasks()
    {
        using var db = CreateDb();
        db.Tasks.AddRange(
            new TodoTask { Title = "Pendente", Status = TodoApi.Models.TaskStatus.Pending, UserId = 1 },
            new TodoTask { Title = "Concluída", Status = TodoApi.Models.TaskStatus.Done, UserId = 1 }
        );
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).GetAll("Done");

        var ok = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponse>>(ok.Value);
        Assert.Single(tasks);
        Assert.Equal("Concluída", tasks.First().Title);
    }

    [Fact]
    public async Task GetAll_WithInvalidStatusFilter_ReturnsAllTasks()
    {
        using var db = CreateDb();
        db.Tasks.AddRange(
            new TodoTask { Title = "Tarefa 1", UserId = 1 },
            new TodoTask { Title = "Tarefa 2", UserId = 1 }
        );
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).GetAll("StatusInvalido");

        var ok = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskResponse>>(ok.Value);
        Assert.Equal(2, tasks.Count());
    }

    // ── GET /api/tasks/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenTaskExists_ReturnsTask()
    {
        using var db = CreateDb();
        var task = new TodoTask { Title = "Minha tarefa", UserId = 1 };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).GetById(task.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TaskResponse>(ok.Value);
        Assert.Equal("Minha tarefa", response.Title);
    }

    [Fact]
    public async Task GetById_WhenTaskNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var result = await CreateController(db, userId: 1).GetById(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_WhenTaskBelongsToOtherUser_ReturnsNotFound()
    {
        using var db = CreateDb();
        var task = new TodoTask { Title = "Tarefa alheia", UserId = 2 };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).GetById(task.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    // ── POST /api/tasks ───────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        using var db = CreateDb();
        var request = new CreateTaskRequest("Nova tarefa", "Descrição da tarefa", null);

        var result = await CreateController(db, userId: 1).Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<TaskResponse>(created.Value);
        Assert.Equal("Nova tarefa", response.Title);
        Assert.Equal("Pending", response.Status);
    }

    // ── PUT /api/tasks/{id} ───────────────────────────────────────────────────

    [Fact]
    public async Task Update_WhenTaskNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var result = await CreateController(db, userId: 1)
            .Update(999, new UpdateTaskRequest("Novo título", null, null, null));
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_WithNewTitle_ReturnsUpdatedTask()
    {
        using var db = CreateDb();
        var task = new TodoTask { Title = "Título original", UserId = 1 };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1)
            .Update(task.Id, new UpdateTaskRequest("Título atualizado", null, null, null));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TaskResponse>(ok.Value);
        Assert.Equal("Título atualizado", response.Title);
    }

    [Fact]
    public async Task Update_WithNewStatus_ReturnsTaskWithUpdatedStatus()
    {
        using var db = CreateDb();
        var task = new TodoTask { Title = "Tarefa", Status = TodoApi.Models.TaskStatus.Pending, UserId = 1 };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1)
            .Update(task.Id, new UpdateTaskRequest(null, null, "InProgress", null));

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TaskResponse>(ok.Value);
        Assert.Equal("InProgress", response.Status);
    }

    // ── DELETE /api/tasks/{id} ────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WhenTaskExists_ReturnsNoContent()
    {
        using var db = CreateDb();
        var task = new TodoTask { Title = "Para deletar", UserId = 1 };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        var result = await CreateController(db, userId: 1).Delete(task.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await db.Tasks.CountAsync());
    }

    [Fact]
    public async Task Delete_WhenTaskNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var result = await CreateController(db, userId: 1).Delete(999);
        Assert.IsType<NotFoundResult>(result);
    }
}
