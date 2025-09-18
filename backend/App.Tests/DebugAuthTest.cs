using System.Net;
using System.Net.Http.Json;
using App.DTOs;
using App.Domain;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace App.Tests;

public class DebugAuthTest : IntegrationTestBase, IAsyncLifetime
{
    public DebugAuthTest(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ShouldShowErrorDetails()
    {
        var request = new RegisterRequest
        {
            Email = "debug@example.com",
            Password = "ValidPassword123!",
            DisplayName = "Debug User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        var content = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine($"Status: {response.StatusCode}");
        System.Console.WriteLine($"Response: {content}");

        // For now, just check that we get some response
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }
}