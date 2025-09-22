using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using App.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace App.Tests;

public class ChatFunctionalityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ChatFunctionalityTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SendMessage_ValidRequest_ReturnsCreatedMessage()
    {
        // Arrange - Create test users and match
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var sendMessageRequest = new SendMessageRequest
        {
            MatchId = matchId,
            Text = "Hello Bob! How are you?"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matches/{matchId}/messages", sendMessageRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var message = await response.Content.ReadFromJsonAsync<MessageResponse>();
        Assert.NotNull(message);
        Assert.Equal(sendMessageRequest.Text, message.Content);
        Assert.Equal(alice, message.SenderId);
        Assert.Equal(matchId, message.MatchId);
    }

    [Fact]
    public async Task SendMessage_EmptyText_ReturnsBadRequest()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var sendMessageRequest = new SendMessageRequest
        {
            MatchId = matchId,
            Text = "" // Empty text should fail
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matches/{matchId}/messages", sendMessageRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SendMessage_UserNotInMatch_ReturnsBadRequest()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        var charlie = await CreateTestUser("charlie@test.com", "Charlie");
        var accessToken = await GetAccessTokenForUser(charlie); // Charlie not in match
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var sendMessageRequest = new SendMessageRequest
        {
            MatchId = matchId,
            Text = "Hello from Charlie"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/matches/{matchId}/messages", sendMessageRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMessages_ValidMatch_ReturnsMessages()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();

        // Send some test messages
        await SendTestMessage(alice, matchId, "Hello Bob!");
        await SendTestMessage(bob, matchId, "Hi Alice!");
        await SendTestMessage(alice, matchId, "How are you?");

        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.GetAsync($"/api/matches/{matchId}/messages");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<MessageResponse>>();
        Assert.NotNull(messages);
        Assert.Equal(3, messages.Count);

        // Messages should be ordered by sent time
        Assert.Equal("Hello Bob!", messages[0].Content);
        Assert.Equal("Hi Alice!", messages[1].Content);
        Assert.Equal("How are you?", messages[2].Content);
    }

    [Fact]
    public async Task GetMessages_UserNotInMatch_ReturnsEmptyList()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        var charlie = await CreateTestUser("charlie@test.com", "Charlie");

        await SendTestMessage(alice, matchId, "Secret message");

        var accessToken = await GetAccessTokenForUser(charlie); // Charlie not in match
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.GetAsync($"/api/matches/{matchId}/messages");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<MessageResponse>>();
        Assert.NotNull(messages);
        Assert.Empty(messages); // Charlie shouldn't see any messages
    }

    [Fact]
    public async Task GetMessages_WithPagination_ReturnsCorrectSubset()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();

        // Send 10 test messages
        for (int i = 1; i <= 10; i++)
        {
            await SendTestMessage(alice, matchId, $"Message {i}");
            await Task.Delay(10); // Small delay to ensure different timestamps
        }

        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act - Get first 5 messages
        var response = await _client.GetAsync($"/api/matches/{matchId}/messages?take=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<MessageResponse>>();
        Assert.NotNull(messages);
        Assert.Equal(5, messages.Count);

        // Should get the 5 most recent messages
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal($"Message {10 - i}", messages[i].Content);
        }
    }

    [Fact]
    public async Task GetMatches_ReturnsMatchesWithLastMessage()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        await SendTestMessage(bob, matchId, "Latest message from Bob");

        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.GetAsync("/api/matches");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var matches = await response.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.NotNull(matches);
        Assert.Single(matches);

        var match = matches[0];
        Assert.Equal(matchId, match.MatchId);
        Assert.Equal("Latest message from Bob", match.LastMessagePreview);
        Assert.Equal("Bob", match.Counterpart.DisplayName);
    }

    [Fact]
    public async Task UnmatchUser_ValidMatch_RemovesMatch()
    {
        // Arrange
        var (alice, bob, matchId) = await CreateTestUsersAndMatch();
        await SendTestMessage(alice, matchId, "Hello Bob!");

        var accessToken = await GetAccessTokenForUser(alice);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await _client.DeleteAsync($"/api/matches/{matchId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify match is removed
        var matchesResponse = await _client.GetAsync("/api/matches");
        var matches = await matchesResponse.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.NotNull(matches);
        Assert.Empty(matches);

        // Verify messages are no longer accessible
        var messagesResponse = await _client.GetAsync($"/api/matches/{matchId}/messages");
        var messages = await messagesResponse.Content.ReadFromJsonAsync<List<MessageResponse>>();
        Assert.NotNull(messages);
        Assert.Empty(messages);
    }

    private async Task<(Guid aliceId, Guid bobId, Guid matchId)> CreateTestUsersAndMatch()
    {
        var alice = await CreateTestUser("alice@test.com", "Alice");
        var bob = await CreateTestUser("bob@test.com", "Bob");

        // Create profiles for both users
        await CreateTestProfile(alice, "Alice", DateTime.Now.AddYears(-25));
        await CreateTestProfile(bob, "Bob", DateTime.Now.AddYears(-28));

        // Create mutual swipes
        await CreateSwipe(alice, bob, isLike: true);
        await CreateSwipe(bob, alice, isLike: true);

        // Create match
        var matchId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var match = new Domain.Match
        {
            Id = matchId,
            UserAId = alice < bob ? alice : bob,
            UserBId = alice < bob ? bob : alice,
            CreatedAt = DateTime.UtcNow
        };

        context.Matches.Add(match);
        await context.SaveChangesAsync();

        return (alice, bob, matchId);
    }

    private async Task<Guid> CreateTestUser(string email, string displayName)
    {
        var userId = Guid.NewGuid();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var user = new Microsoft.AspNetCore.Identity.IdentityUser<Guid>
        {
            Id = userId,
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            EmailConfirmed = true,
            PasswordHash = "AQAAAAEAACcQAAAAEDqugKZkIKVKlkWThWHjDsHwiSBertNHdfw/k3IdGsuY6t62vpypM48Td0H8AYPvRQ=="
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return userId;
    }

    private async Task CreateTestProfile(Guid userId, string displayName, DateTime birthDate)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var profile = new Domain.Profile
        {
            UserId = userId,
            DisplayName = displayName,
            BirthDate = birthDate,
            Gender = Domain.Gender.Unknown,
            SearchGender = Domain.Gender.Unknown,
            AgeMin = 18,
            AgeMax = 99,
            MaxDistanceKm = 50
        };

        context.Profiles.Add(profile);
        await context.SaveChangesAsync();
    }

    private async Task CreateSwipe(Guid swiperId, Guid targetId, bool isLike)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var swipe = new Domain.Swipe
        {
            Id = Guid.NewGuid(),
            SwiperId = swiperId,
            TargetId = targetId,
            Direction = isLike ? Domain.SwipeDirection.Like : Domain.SwipeDirection.Pass,
            CreatedAt = DateTime.UtcNow
        };

        context.Swipes.Add(swipe);
        await context.SaveChangesAsync();
    }

    private async Task SendTestMessage(Guid senderId, Guid matchId, string text)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var message = new Domain.Message
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            SenderId = senderId,
            Content = text,
            SentAt = DateTime.UtcNow,
            DeliveredAt = DateTime.UtcNow
        };

        context.Messages.Add(message);
        await context.SaveChangesAsync();
    }

    private async Task<string> GetAccessTokenForUser(Guid userId)
    {
        // In a real test, you'd generate a proper JWT token
        // For now, we'll use the test authentication setup
        return "test-token"; // This would need to be implemented based on your auth setup
    }
}