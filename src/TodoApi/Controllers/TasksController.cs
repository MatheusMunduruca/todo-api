using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos os endpoints exigem JWT válido
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    // Método auxiliar: extrai o UserId do token JWT
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Método auxiliar: converte Model -> DTO de resposta
    private static TaskResponse ToResponse(TodoTask t) =>
        new(t.Id, t.Title, t.Description, t.Status.ToString(), t.CreatedAt, t.DueDate);

    /// <summary>
    /// GET /api/tasks
    /// Lista todas as tarefas do usuário autenticado.
    /// Suporta filtro por status: ?status=Pending
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = _db.Tasks.Where(t => t.UserId == GetUserId());

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<TodoApi.Models.TaskStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToResponse(t))
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// GET /api/tasks/{id}
    /// Retorna uma tarefa específica. Retorna 404 se não existir ou não pertencer ao usuário.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == GetUserId());

        if (task is null) return NotFound();
        return Ok(ToResponse(task));
    }

    /// <summary>
    /// POST /api/tasks
    /// Cria uma nova tarefa para o usuário autenticado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest req)
    {
        var task = new TodoTask
        {
            Title = req.Title,
            Description = req.Description,
            DueDate = req.DueDate,
            UserId = GetUserId()
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        // Retorna 201 Created com a localização do novo recurso
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, ToResponse(task));
    }

    /// <summary>
    /// PUT /api/tasks/{id}
    /// Atualiza campos de uma tarefa existente (parcialmente).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest req)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == GetUserId());

        if (task is null) return NotFound();

        if (!string.IsNullOrEmpty(req.Title)) task.Title = req.Title;
        if (req.Description is not null) task.Description = req.Description;
        if (req.DueDate is not null) task.DueDate = req.DueDate;

        if (!string.IsNullOrEmpty(req.Status) &&
            Enum.TryParse<TodoApi.Models.TaskStatus>(req.Status, true, out var parsedStatus))
        {
            task.Status = parsedStatus;
        }

        await _db.SaveChangesAsync();
        return Ok(ToResponse(task));
    }

    /// <summary>
    /// DELETE /api/tasks/{id}
    /// Remove uma tarefa. Retorna 204 No Content em caso de sucesso.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == GetUserId());

        if (task is null) return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
