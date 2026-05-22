namespace TodoApi.DTOs;

// ── Auth ──────────────────────────────────────────────
public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string Name, string Email);

// ── Tasks ─────────────────────────────────────────────
public record CreateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate
);

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? Status,   // "Pending" | "InProgress" | "Done"
    DateTime? DueDate
);

public record TaskResponse(
    int Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? DueDate
);
