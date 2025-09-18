using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using App.DTOs;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using App.Domain;

namespace App.Tests.Controllers;

public class FeedControllerTests : IntegrationTestBase, IAsyncLifetime
{
    public FeedControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCandidates_WithAuth_ShouldReturnCandidates()
    {
        await CreateMultipleUsers(5);
        var token = await CreateAndAuthenticateUser("feed@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidates = await response.Content.ReadFromJsonAsync<FeedResponse>();
        candidates.Should().NotBeNull();
        candidates!.Candidates.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCandidates_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/feed");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCandidates_WithRadiusFilter_ShouldReturnFilteredResults()
    {
        await CreateMultipleUsers(3);
        var token = await CreateAndAuthenticateUser("radius@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/feed?radiusKm=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidates = await response.Content.ReadFromJsonAsync<FeedResponse>();
        candidates.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCandidates_WithPagination_ShouldReturnPagedResults()
    {
        await CreateMultipleUsers(10);
        var token = await CreateAndAuthenticateUser("paging@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/feed?page=1&pageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidates = await response.Content.ReadFromJsonAsync<FeedResponse>();
        candidates.Should().NotBeNull();
        candidates!.Candidates.Should().HaveCountLessOrEqualTo(3);
    }

    [Fact]
    public async Task GetCandidates_ShouldExcludeAlreadySwipedUsers()
    {
        var (otherToken, otherId) = await CreateUserAndGetInfo("other@test.com");
        var token = await CreateAndAuthenticateUser("swiper@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsJsonAsync("/api/swipes", new SwipeRequest
        {
            TargetUserId = otherId,
            IsLike = false
        });

        var response = await Client.GetAsync("/api/feed");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidates = await response.Content.ReadFromJsonAsync<FeedResponse>();
        candidates.Should().NotBeNull();
        candidates!.Candidates.Should().NotContain(c => c.UserId == otherId);
    }

    [Fact]
    public async Task GetCandidates_WithMaxPageSizeExceeded_ShouldCapAt50()
    {
        await CreateMultipleUsers(60);
        var token = await CreateAndAuthenticateUser("maxpage@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/feed?pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var candidates = await response.Content.ReadFromJsonAsync<FeedResponse>();
        candidates.Should().NotBeNull();
        candidates!.Candidates.Should().HaveCountLessOrEqualTo(50);
    }

    private async Task CreateMultipleUsers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await RegisterUserAsync($"user{i}@test.com", "TestPassword123!", $"User {i}");
        }
    }

    private async Task<string> CreateAndAuthenticateUser(string email)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "TestPassword123!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "TestPassword123!"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    private async Task<(string token, Guid userId)> CreateUserAndGetInfo(string email)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "TestPassword123!",
            DisplayName = email.Split('@')[0],
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Female
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "TestPassword123!"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return (authResponse!.AccessToken, registerResult!.UserId);
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
    }

    private class RegisterResponse
    {
        public Guid UserId { get; set; }
    }

    private class FeedResponse
    {
        public List<CandidateDto> Candidates { get; set; } = new();
    }

    private class CandidateDto
    {
        public Guid UserId { get; set; }
    }

    private class SwipeRequest
    {
        public Guid TargetUserId { get; set; }
        public bool IsLike { get; set; }
    }

    private class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    private class RegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public Gender Gender { get; set; }
    }
}