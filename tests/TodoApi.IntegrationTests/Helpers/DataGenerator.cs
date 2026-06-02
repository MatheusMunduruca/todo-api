using Bogus;
using TodoApi.DTOs;

namespace TodoApi.IntegrationTests.Helpers;

/// <summary>
/// Gera dados realistas e aleatórios para os testes usando a biblioteca Bogus.
/// </summary>
public static class DataGenerator
{
    private static readonly Faker Faker = new("pt_BR");

    public static RegisterRequest NewUser() => new(
        Faker.Name.FullName(),
        Faker.Internet.Email(),
        Faker.Internet.Password(12)
    );

    public static LoginRequest LoginFrom(RegisterRequest user) => new(
        user.Email,
        user.Password
    );

    public static CreateTaskRequest NewTask() => new(
        Faker.Lorem.Sentence(3).TrimEnd('.'),
        Faker.Lorem.Sentence(8),
        Faker.Date.Future()
    );

    public static CreateTaskRequest NewTaskWithoutDescription() => new(
        Faker.Lorem.Sentence(3).TrimEnd('.'),
        null,
        null
    );

    public static UpdateTaskRequest UpdateWith(string status) => new(
        null, null, status, null
    );
}
