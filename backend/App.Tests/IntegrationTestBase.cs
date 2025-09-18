using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using App.Tests.Fixtures;
using App.DTOs;
using App.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace App.Tests;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly HttpClient Client;
    protected readonly TestWebApplicationFactory Factory;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<string> AuthenticateAsync(string email = "test@example.com", string password = "TestPassword123!")
    {
        await RegisterUserAsync(email, password);
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        loginResponse.EnsureSuccessStatusCode();
        var content = await loginResponse.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, JsonOptions);
        return tokenResponse!.AccessToken;
    }

    protected async Task<HttpResponseMessage> RegisterUserAsync(string email, string password, string? name = null)
    {
        var registerDto = new RegisterRequest
        {
            Email = email,
            Password = password,
            DisplayName = name ?? "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = App.Domain.Gender.Male
        };

        return await Client.PostAsJsonAsync("/api/auth/register", registerDto);
    }

    protected void AuthorizeClient(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task ResetDatabaseAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    private class TokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}