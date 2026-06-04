[![CI](https://github.com/MatheusMunduruca/todo-api/actions/workflows/ci.yml/badge.svg)](https://github.com/MatheusMunduruca/todo-api/actions/workflows/ci.yml)
# 📝 Todo API

API REST para gerenciamento de tarefas com autenticação JWT, construída em C# .NET 8.

![CI](https://github.com/MatheusMunduruca/todo-api/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-blue?logo=csharp)
![MySQL](https://img.shields.io/badge/MySQL-8.0-orange?logo=mysql)
![Docker](https://img.shields.io/badge/Docker-Compose-blue?logo=docker)
![Tests](https://img.shields.io/badge/Tests-45%20passing-brightgreen?logo=checkmarx)
![Swagger](https://img.shields.io/badge/Docs-Swagger-green?logo=swagger)

---

## 🌌 Universo compartilhado

A Todo API (Taverna do Gregor) compartilha **usuários e economia de ouro** com o **[Empório do Rudolf](https://github.com/MatheusMunduruca/ecommerce-api)** (e-commerce de alquimia):

- **Login único:** ambas as APIs usam a **mesma chave JWT** e a **mesma tabela de usuários** (`todo_db`), então uma conta funciona nos dois sistemas.
- **Economia única:** o saldo de **Gold Coins** fica em `Users.GoldBalance` e é compartilhado — ganha-se ouro cumprindo tarefas aqui e gasta-se comprando itens no Empório.
- Novas contas recebem **1.000 de ouro** de boas-vindas.

---

## 🚀 Funcionalidades

- ✅ Registro e autenticação de usuários com JWT Bearer
- ✅ Hash de senha com BCrypt
- ✅ CRUD completo de tarefas
- ✅ Filtro de tarefas por status
- ✅ Isolamento por usuário (cada usuário vê apenas suas tarefas)
- ✅ **Economia de ouro compartilhada** com o Empório do Rudolf (consultar / creditar / deduzir)
- ✅ Documentação interativa via Swagger/OpenAPI
- ✅ Containerização com Docker e Docker Compose
- ✅ 20 testes unitários (xUnit + Moq)
- ✅ 22 testes de integração (WebApplicationFactory + FluentAssertions + Bogus)
- ✅ 3 cenários BDD (SpecFlow / Gherkin)
- ✅ CI no GitHub Actions (build + testes a cada push/PR)

---

## 🛠️ Tech Stack

| Categoria | Tecnologia |
|-----------|-----------|
| **Framework** | .NET 8 / ASP.NET Core |
| **ORM** | Entity Framework Core + Pomelo MySQL Driver |
| **Autenticação** | JWT Bearer + BCrypt |
| **Banco de Dados** | MySQL 8.0 |
| **Container** | Docker + Docker Compose |
| **Documentação** | Swagger / OpenAPI |
| **Testes Unitários** | xUnit + Moq + EF Core InMemory |
| **Testes de Integração** | WebApplicationFactory + FluentAssertions + Bogus |
| **Testes BDD** | SpecFlow (Gherkin) |
| **CI/CD** | GitHub Actions |

---

## 📁 Estrutura do Projeto

```
todo-api/
├── src/
│   └── TodoApi/
│       ├── Controllers/
│       │   ├── AuthController.cs       # POST /api/auth/register, /login
│       │   └── TasksController.cs      # CRUD /api/tasks (JWT obrigatório)
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── DTOs/
│       │   └── Dtos.cs                 # RegisterRequest, LoginRequest, AuthResponse,
│       │                               # CreateTaskRequest, UpdateTaskRequest, TaskResponse
│       ├── Models/
│       │   ├── User.cs
│       │   └── TodoTask.cs             # enum TaskStatus { Pending, InProgress, Done }
│       ├── Services/
│       │   └── TokenService.cs         # Gera JWT com claims
│       ├── Migrations/
│       ├── Program.cs
│       └── appsettings.json
│
└── tests/
    ├── TodoApi.Tests/                  # Testes unitários — 20 testes
    │   ├── Controllers/
    │   │   ├── AuthControllerTests.cs
    │   │   └── TasksControllerTests.cs
    │   └── Services/
    │       └── TokenServiceTests.cs
    │
    └── TodoApi.IntegrationTests/       # Testes de integração — 22 testes
        ├── Auth/
        │   └── AuthIntegrationTests.cs
        ├── Tasks/
        │   └── TasksIntegrationTests.cs
        ├── Fixtures/
        │   └── TodoApiFactory.cs       # WebApplicationFactory<Program> com EF Core InMemory
        └── Helpers/
            └── DataGenerator.cs        # Bogus para dados realistas
```

---

## 📡 Endpoints

### Auth

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `POST` | `/api/auth/register` | Cria usuário, concede 1.000 de ouro, retorna JWT | ❌ |
| `POST` | `/api/auth/login` | Autentica usuário, retorna JWT + saldo | ❌ |
| `GET`  | `/api/auth/gold` | Saldo de ouro do usuário | ✅ |
| `POST` | `/api/auth/gold/add` | Credita ouro (recompensa de tarefa) | ✅ |
| `POST` | `/api/auth/gold/deduct` | Deduz ouro | ✅ |

### Tasks

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| `GET` | `/api/tasks` | Lista tarefas do usuário (filtro opcional por status) | ✅ |
| `GET` | `/api/tasks/{id}` | Busca tarefa por ID | ✅ |
| `POST` | `/api/tasks` | Cria nova tarefa | ✅ |
| `PUT` | `/api/tasks/{id}` | Atualiza tarefa | ✅ |
| `DELETE` | `/api/tasks/{id}` | Remove tarefa | ✅ |

**Filtro de status disponível:**
```
GET /api/tasks?status=Pending
GET /api/tasks?status=InProgress
GET /api/tasks?status=Done
```

---

## 🧪 Testes

O projeto possui **45 testes automatizados** divididos em três níveis, executados a cada push pelo GitHub Actions:

### Testes BDD (3 cenários) — `TodoApi.BddTests`

Usando **SpecFlow (Gherkin)**. Descrevem o gerenciamento de missões em linguagem de negócio
(criar missão, concluir missão, exigência de autenticação), executando contra a API real em memória.

### Testes Unitários (20 testes) — `TodoApi.Tests`

Usando **xUnit + Moq + EF Core InMemory**. Cobrem:

- `AuthController` — register com dados válidos/duplicados/inválidos, login válido/inválido
- `TasksController` — CRUD com mock de DbContext, isolamento por UserId
- `TokenService` — geração de token, validação de claims

### Testes de Integração (22 testes) — `TodoApi.IntegrationTests`

Usando **xUnit + WebApplicationFactory + FluentAssertions + Bogus**.
Banco InMemory por instância de factory (isolamento total entre testes).

**Auth (7 testes):**
- Register com dados válidos → 200 + token
- Register com email duplicado → 409
- Register com email inválido → 400
- Register → token gerado tem formato JWT válido (3 partes)
- Login com credenciais válidas → 200 + token
- Login com senha errada → 401
- Login com email inexistente → 401

**Tasks (15 testes):**
- GET sem token → 401
- POST sem token → 401
- GET usuário sem tarefas → lista vazia
- GET retorna apenas tarefas do usuário autenticado (isolamento)
- GET com filtro `?status=InProgress` → só as correspondentes
- POST dados válidos → 201 com tarefa criada
- POST sem descrição → 201
- GET por ID existente → 200
- GET por ID inexistente → 404
- GET por ID de outro usuário → 404
- PUT alterando status → 200 com novo status
- PUT tarefa inexistente → 404
- DELETE existente → 204
- DELETE existente → some da lista
- DELETE inexistente → 404

### Executar os testes

```bash
# Todos os testes
dotnet test

# Apenas unitários
dotnet test tests/TodoApi.Tests

# Apenas integração
dotnet test tests/TodoApi.IntegrationTests

# Com detalhes
dotnet test --verbosity normal
```

---

## ⚙️ Como rodar localmente

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Clone o repositório

```bash
git clone https://github.com/MatheusMunduruca/todo-api.git
cd todo-api
```

### 2. Suba o banco de dados MySQL

```bash
docker-compose up -d
```

### 3. Configure as variáveis de ambiente

Crie um arquivo `appsettings.Development.json` em `src/TodoApi/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=tododb;User=root;Password=root;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-muito-segura-aqui",
    "Issuer": "TodoApi",
    "Audience": "TodoApiUsers"
  }
}
```

### 4. Execute as migrations

```bash
cd src/TodoApi
dotnet ef database update
```

### 5. Rode a API

```bash
dotnet run
```

A API estará disponível em:
- **HTTP:** `http://localhost:5000`
- **Swagger:** `http://localhost:5000/swagger`

---

## 📖 Documentação Interativa

Acesse o Swagger UI em `http://localhost:5000/swagger` para explorar e testar todos os endpoints diretamente pelo browser.

---

## 🔐 Autenticação

A API utiliza **JWT Bearer Token**. Para acessar endpoints protegidos:

1. Faça register ou login
2. Copie o token retornado
3. No Swagger, clique em **Authorize** e insira: `Bearer {seu_token}`
4. Ou adicione o header: `Authorization: Bearer {seu_token}`

---

## 🐳 Docker

O projeto utiliza Docker Compose para subir o MySQL:

```bash
# Subir MySQL
docker-compose up -d

# Parar MySQL
docker-compose down

# Ver logs
docker-compose logs -f
```

---

## 📝 Histórico de Commits

```
feat: add integration tests with WebApplicationFactory (22 tests, all passing)
feat: add CORS configuration for frontend integration
chore: clean up gitignore
feat: add unit tests (xUnit + Moq) - 20 tests covering auth and tasks
feat: initial commit - Todo API with JWT auth and Docker
```

---

## 🌐 CORS

API configurada com CORS para integração com frontend em `http://localhost:5173`.

---

## 👨‍💻 Autor

**Matheus Munduruca**

[![GitHub](https://img.shields.io/badge/GitHub-MatheusMunduruca-black?logo=github)](https://github.com/MatheusMunduruca)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-matheusmunduruca-blue?logo=linkedin)](linkedin.com/in/matheusmunduruca)

---

## 📄 Licença

Este projeto está sob a licença MIT.
