namespace TodoApi.Models;

public enum TaskStatus
{
    Pending,
    InProgress,
    Done
}

public class TodoTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Chave estrangeira para User
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
