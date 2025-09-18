using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using App.DTOs;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using App.Domain;

namespace App.Tests.Controllers;

public class SwipesAndMatchesControllerTests : IntegrationTestBase, IAsyncLifetime
{
    public SwipesAndMatchesControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ProcessSwipe_WithValidLike_ShouldCreateSwipe()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("user1@test.com");
        var (_, user2Id) = await CreateUserAndGetInfo("user2@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var swipeRequest = new SwipeRequest
        {
            TargetUserId = user2Id,
            IsLike = true
        };

        var response = await Client.PostAsJsonAsync("/api/swipes", swipeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SwipeResult>();
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessSwipe_WithMutualLike_ShouldCreateMatch()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("user1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("user2@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
        var swipe1Request = new SwipeRequest
        {
            TargetUserId = user2Id,
            IsLike = true
        };
        await Client.PostAsJsonAsync("/api/swipes", swipe1Request);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
        var swipe2Request = new SwipeRequest
        {
            TargetUserId = user1Id,
            IsLike = true
        };

        var response = await Client.PostAsJsonAsync("/api/swipes", swipe2Request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SwipeResult>();
        result.Should().NotBeNull();
        result!.IsMatch.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessSwipe_WithoutAuth_ShouldReturnUnauthorized()
    {
        var (_, userId) = await CreateUserAndGetInfo("target@test.com");

        var swipeRequest = new SwipeRequest
        {
            TargetUserId = userId,
            IsLike = true
        };

        var response = await Client.PostAsJsonAsync("/api/swipes", swipeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProcessSwipe_WithSelfSwipe_ShouldReturnBadRequest()
    {
        var (userToken, userId) = await CreateUserAndGetInfo("self@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var swipeRequest = new SwipeRequest
        {
            TargetUserId = userId,
            IsLike = true
        };

        var response = await Client.PostAsJsonAsync("/api/swipes", swipeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMatches_WithMatches_ShouldReturnList()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("match1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("match2@test.com");
        var (user3Token, user3Id) = await CreateUserAndGetInfo("match3@test.com");

        await CreateMatch(user1Token, user2Token, user1Id, user2Id);
        await CreateMatch(user1Token, user3Token, user1Id, user3Id);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var response = await Client.GetAsync("/api/matches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var matches = await response.Content.ReadFromJsonAsync<MatchListResponse>();
        matches.Should().NotBeNull();
        matches!.Matches.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMatches_WithPagination_ShouldReturnPagedResults()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("paged1@test.com");

        for (int i = 0; i < 5; i++)
        {
            var (otherToken, otherId) = await CreateUserAndGetInfo($"other{i}@test.com");
            await CreateMatch(user1Token, otherToken, user1Id, otherId);
        }

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var response = await Client.GetAsync("/api/matches?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var matches = await response.Content.ReadFromJsonAsync<MatchListResponse>();
        matches.Should().NotBeNull();
        matches!.Matches.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMatchDetails_WithValidMatch_ShouldReturnDetails()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("detail1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("detail2@test.com");

        var matchId = await CreateMatch(user1Token, user2Token, user1Id, user2Id);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var response = await Client.GetAsync($"/api/matches/{matchId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var match = await response.Content.ReadFromJsonAsync<MatchDetailsDto>();
        match.Should().NotBeNull();
        match!.Id.Should().Be(matchId);
    }

    [Fact]
    public async Task GetMatchDetails_WithInvalidMatch_ShouldReturnNotFound()
    {
        var (userToken, _) = await CreateUserAndGetInfo("notfound@test.com");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var response = await Client.GetAsync($"/api/matches/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendMessage_WithValidMatch_ShouldSendSuccessfully()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("msg1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("msg2@test.com");

        var matchId = await CreateMatch(user1Token, user2Token, user1Id, user2Id);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var messageRequest = new SendMessageRequest
        {
            Content = "Hello there!"
        };

        var response = await Client.PostAsJsonAsync($"/api/matches/{matchId}/messages", messageRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var message = await response.Content.ReadFromJsonAsync<MessageDto>();
        message.Should().NotBeNull();
        message!.Content.Should().Be("Hello there!");
    }

    [Fact]
    public async Task GetMessages_WithMessages_ShouldReturnList()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("msglist1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("msglist2@test.com");

        var matchId = await CreateMatch(user1Token, user2Token, user1Id, user2Id);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
        await Client.PostAsJsonAsync($"/api/matches/{matchId}/messages", new SendMessageRequest { Content = "Message 1" });
        await Client.PostAsJsonAsync($"/api/matches/{matchId}/messages", new SendMessageRequest { Content = "Message 2" });

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
        await Client.PostAsJsonAsync($"/api/matches/{matchId}/messages", new SendMessageRequest { Content = "Reply 1" });

        var response = await Client.GetAsync($"/api/matches/{matchId}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
        messages.Should().NotBeNull();
        messages!.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMessages_WithPagination_ShouldReturnPagedResults()
    {
        var (user1Token, user1Id) = await CreateUserAndGetInfo("msgpage1@test.com");
        var (user2Token, user2Id) = await CreateUserAndGetInfo("msgpage2@test.com");

        var matchId = await CreateMatch(user1Token, user2Token, user1Id, user2Id);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
        for (int i = 0; i < 10; i++)
        {
            await Client.PostAsJsonAsync($"/api/matches/{matchId}/messages", new SendMessageRequest { Content = $"Message {i}" });
        }

        var response = await Client.GetAsync($"/api/matches/{matchId}/messages?take=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messages = await response.Content.ReadFromJsonAsync<List<MessageDto>>();
        messages.Should().NotBeNull();
        messages!.Should().HaveCountLessOrEqualTo(5);
    }

    private async Task<(string token, Guid userId)> CreateUserAndGetInfo(string email)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "TestPassword123!",
            DisplayName = email.Split('@')[0],
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
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

    private async Task<Guid> CreateMatch(string user1Token, string user2Token, Guid user1Id, Guid user2Id)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
        await Client.PostAsJsonAsync("/api/swipes", new SwipeRequest { TargetUserId = user2Id, IsLike = true });

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
        var response = await Client.PostAsJsonAsync("/api/swipes", new SwipeRequest { TargetUserId = user1Id, IsLike = true });

        var result = await response.Content.ReadFromJsonAsync<SwipeResult>();
        return result!.MatchId!.Value;
    }

    private class RegisterResponse
    {
        public Guid UserId { get; set; }
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }

    private class SwipeRequest
    {
        public Guid TargetUserId { get; set; }
        public bool IsLike { get; set; }
    }

    private class SwipeResult
    {
        public bool IsMatch { get; set; }
        public Guid? MatchId { get; set; }
    }

    private class MatchListResponse
    {
        public List<MatchDto> Matches { get; set; } = new();
    }

    private class MatchDto
    {
        public Guid Id { get; set; }
    }

    private class MatchDetailsDto
    {
        public Guid Id { get; set; }
    }

    private class SendMessageRequest
    {
        public string Content { get; set; } = default!;
        public Guid? MatchId { get; set; }
    }

    private class MessageDto
    {
        public string Content { get; set; } = default!;
    }
}