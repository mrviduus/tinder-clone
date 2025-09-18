using System.Net;
using System.Net.Http.Json;
using App.DTOs;
using App.Domain;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace App.Tests.Controllers;

public class AuthControllerTests : IntegrationTestBase, IAsyncLifetime
{
    public AuthControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUser()
    {
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "ValidPassword123!",
            DisplayName = "New User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "ValidPassword123!",
            DisplayName = "User One",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Female
        };

        await Client.PostAsJsonAsync("/api/auth/register", request);

        var duplicateRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Password = "AnotherPassword123!",
            DisplayName = "User Two",
            BirthDate = DateTime.UtcNow.AddYears(-26),
            Gender = Gender.Male
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "ValidPassword123!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.NonBinary
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Short1!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        var registerRequest = new RegisterRequest
        {
            Email = "login@example.com",
            Password = "ValidPassword123!",
            DisplayName = "Login User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Female
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "ValidPassword123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
        result.AccessExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.RefreshExpiresAt.Should().BeAfter(result.AccessExpiresAt);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var registerRequest = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "CorrectPassword123!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ShouldReturnNewTokens()
    {
        var registerRequest = new RegisterRequest
        {
            Email = "refresh@example.com",
            Password = "ValidPassword123!",
            DisplayName = "Refresh User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.NonBinary
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "refresh@example.com",
            Password = "ValidPassword123!"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var refreshRequest = new RefreshRequest
        {
            RefreshToken = authResponse!.RefreshToken
        };

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
        result.AccessToken.Should().NotBe(authResponse.AccessToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var refreshRequest = new RefreshRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnNoContent()
    {
        var registerRequest = new RegisterRequest
        {
            Email = "logout@example.com",
            Password = "ValidPassword123!",
            DisplayName = "Logout User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "logout@example.com",
            Password = "ValidPassword123!"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var logoutRequest = new RefreshRequest
        {
            RefreshToken = authResponse!.RefreshToken
        };

        var response = await Client.PostAsJsonAsync("/api/auth/logout", logoutRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshAfterLogout = await Client.PostAsJsonAsync("/api/auth/refresh", logoutRequest);
        refreshAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}