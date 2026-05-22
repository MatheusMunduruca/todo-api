# Todo API

API REST de gerenciamento de tarefas desenvolvida em **C# (.NET 8)** com autenticação **JWT**, banco de dados **MySQL** via **Entity Framework Core** e documentação interativa com **Swagger**.

---

## Tecnologias

- .NET 8 / ASP.NET Core
- Entity Framework Core + Pomelo (MySQL)
- JWT Bearer Authentication
- BCrypt para hash de senhas
- Swagger / OpenAPI
- Docker + Docker Compose
- xUnit + Moq (testes unitários)

---

## Como rodar

### Opção 1 — Docker Compose (recomendado)

```bash
docker-compose up --build
```

A API sobe em `http://localhost:5000/swagger`

### Opção 2 — Rodando local

1. Tenha o .NET 8 SDK e MySQL instalados
2. Ajuste a connection string em `appsettings.json`
3. Execute:

```bash
cd src/TodoApi
dotnet restore
dotnet run
```

Acesse `http://localhost:5000/swagger`

---

## Endpoints

### Auth

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/auth/register` | Registra novo usuário |
| POST | `/api/auth/login` | Autentica e retorna JWT |

### Tasks (requer Authorization: Bearer {token})

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/tasks` | Lista todas as tarefas |
| GET | `/api/tasks?status=Pending` | Filtra por status |
| GET | `/api/tasks/{id}` | Busca tarefa por ID |
| POST | `/api/tasks` | Cria nova tarefa |
| PUT | `/api/tasks/{id}` | Atualiza tarefa |
| DELETE | `/api/tasks/{id}` | Remove tarefa |

---

## Exemplo de uso

### 1. Registrar usuário
```json
POST /api/auth/register
{
  "name": "Matheus",
  "email": "matheus@email.com",
  "password": "senha123"
}
```

### 2. Criar tarefa
```json
POST /api/tasks
Authorization: Bearer {token}

{
  "title": "Estudar Entity Framework",
  "description": "Focar em migrations e relacionamentos",
  "dueDate": "2026-06-01T00:00:00Z"
}
```

### 3. Atualizar status
```json
PUT /api/tasks/1
Authorization: Bearer {token}

{
  "status": "InProgress"
}
```

---

## Status disponíveis

- `Pending` — pendente (padrão)
- `InProgress` — em andamento
- `Done` — concluída

---

## Testes

O projeto possui **20 testes unitários** cobrindo os controllers e serviços.

```bash
cd tests/TodoApi.Tests
dotnet test
```

Cobertura:
- `TokenService` — geração e validação de JWT
- `AuthController` — registro e login (credenciais válidas, inválidas, e-mail duplicado)
- `TasksController` — CRUD completo com isolamento por usuário
