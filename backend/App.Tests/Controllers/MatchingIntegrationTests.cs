using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using App.Data;
using App.Domain;
using App.DTOs;
using App.Tests.Fixtures;

namespace App.Tests.Controllers
{
    public class MatchingIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public MatchingIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SwipeFlow_MutualLike_CreatesMatch()
        {
            // Arrange - Create and authenticate two users
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("swipe1@test.com", "User1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("swipe2@test.com", "User2");

            // Act 1 - User1 likes User2
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            var swipe1Response = await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user2Id, Direction = "like" }));

            swipe1Response.EnsureSuccessStatusCode();
            var swipe1Result = await ParseResponse<SwipeResponse>(swipe1Response);
            Assert.False(swipe1Result.Matched);

            // Act 2 - User2 likes User1 back (should create match)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var swipe2Response = await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user1Id, Direction = "like" }));

            swipe2Response.EnsureSuccessStatusCode();
            var swipe2Result = await ParseResponse<SwipeResponse>(swipe2Response);

            // Assert - Match created
            Assert.True(swipe2Result.Matched);
            Assert.NotNull(swipe2Result.MatchId);

            // Verify match appears in both users' match lists
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            var user1MatchesResponse = await _client.GetAsync("/api/matches");
            user1MatchesResponse.EnsureSuccessStatusCode();
            var user1Matches = await ParseResponse<List<MatchResponse>>(user1MatchesResponse);
            Assert.Single(user1Matches);
            Assert.Equal(swipe2Result.MatchId, user1Matches[0].MatchId);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var user2MatchesResponse = await _client.GetAsync("/api/matches");
            user2MatchesResponse.EnsureSuccessStatusCode();
            var user2Matches = await ParseResponse<List<MatchResponse>>(user2MatchesResponse);
            Assert.Single(user2Matches);
            Assert.Equal(swipe2Result.MatchId, user2Matches[0].MatchId);
        }

        [Fact]
        public async Task MessageFlow_MatchedUsers_CanSendMessages()
        {
            // Arrange - Create match between two users
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("msg1@test.com", "MsgUser1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("msg2@test.com", "MsgUser2");
            var matchId = await CreateMatch(user1Token, user1Id, user2Token, user2Id);

            // Act - User1 sends message
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            var sendMessageResponse = await _client.PostAsync($"/api/matches/{matchId}/messages",
                JsonContent(new SendMessageRequest { MatchId = matchId, Text = "Hello from user 1!" }));

            sendMessageResponse.EnsureSuccessStatusCode();
            var sentMessage = await ParseResponse<MessageResponse>(sendMessageResponse);
            Assert.Equal("Hello from user 1!", sentMessage.Content);
            Assert.Equal(user1Id, sentMessage.SenderId);

            // Act - User2 retrieves messages
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var getMessagesResponse = await _client.GetAsync($"/api/matches/{matchId}/messages");
            getMessagesResponse.EnsureSuccessStatusCode();
            var messages = await ParseResponse<List<MessageResponse>>(getMessagesResponse);

            // Assert
            Assert.Single(messages);
            Assert.Equal("Hello from user 1!", messages[0].Content);
            Assert.Equal(user1Id, messages[0].SenderId);
        }

        [Fact]
        public async Task MessageFlow_UnmatchedUser_CannotSendMessage()
        {
            // Arrange - Create three users, match only user1 and user2
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("nomsg1@test.com", "NoMsgUser1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("nomsg2@test.com", "NoMsgUser2");
            var (user3Token, user3Id) = await CreateAndAuthenticateUser("nomsg3@test.com", "NoMsgUser3");
            var matchId = await CreateMatch(user1Token, user1Id, user2Token, user2Id);

            // Act - User3 tries to send message to match they're not part of
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user3Token);
            var sendMessageResponse = await _client.PostAsync($"/api/matches/{matchId}/messages",
                JsonContent(new SendMessageRequest { MatchId = matchId, Text = "I shouldn't be able to send this" }));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, sendMessageResponse.StatusCode);
        }

        [Fact]
        public async Task UnmatchFlow_ValidMatch_RemovesMatch()
        {
            // Arrange - Create match
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("unmatch1@test.com", "UnmatchUser1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("unmatch2@test.com", "UnmatchUser2");
            var matchId = await CreateMatch(user1Token, user1Id, user2Token, user2Id);

            // Send some messages
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            await _client.PostAsync($"/api/matches/{matchId}/messages",
                JsonContent(new SendMessageRequest { MatchId = matchId, Text = "Hello!" }));

            // Act - User1 unmatches
            var unmatchResponse = await _client.DeleteAsync($"/api/matches/{matchId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, unmatchResponse.StatusCode);

            // Verify match no longer exists for either user
            var user1MatchesResponse = await _client.GetAsync("/api/matches");
            user1MatchesResponse.EnsureSuccessStatusCode();
            var user1Matches = await ParseResponse<List<MatchResponse>>(user1MatchesResponse);
            Assert.Empty(user1Matches);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var user2MatchesResponse = await _client.GetAsync("/api/matches");
            user2MatchesResponse.EnsureSuccessStatusCode();
            var user2Matches = await ParseResponse<List<MatchResponse>>(user2MatchesResponse);
            Assert.Empty(user2Matches);
        }

        [Fact]
        public async Task SwipeFlow_PassDirection_NoMatch()
        {
            // Arrange
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("pass1@test.com", "PassUser1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("pass2@test.com", "PassUser2");

            // Act - User1 passes on User2
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            var swipeResponse = await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user2Id, Direction = "pass" }));

            swipeResponse.EnsureSuccessStatusCode();
            var swipeResult = await ParseResponse<SwipeResponse>(swipeResponse);

            // Assert
            Assert.False(swipeResult.Matched);
            Assert.Null(swipeResult.MatchId);

            // Even if User2 likes User1, no match should be created
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var swipe2Response = await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user1Id, Direction = "like" }));

            swipe2Response.EnsureSuccessStatusCode();
            var swipe2Result = await ParseResponse<SwipeResponse>(swipe2Response);
            Assert.False(swipe2Result.Matched);
        }

        [Fact]
        public async Task GetMatches_MultipleMatches_ReturnsSortedByActivity()
        {
            // Arrange - Create user with multiple matches
            var (user1Token, user1Id) = await CreateAndAuthenticateUser("multi1@test.com", "MultiUser1");
            var (user2Token, user2Id) = await CreateAndAuthenticateUser("multi2@test.com", "MultiUser2");
            var (user3Token, user3Id) = await CreateAndAuthenticateUser("multi3@test.com", "MultiUser3");

            var match1Id = await CreateMatch(user1Token, user1Id, user2Token, user2Id);
            var match2Id = await CreateMatch(user1Token, user1Id, user3Token, user3Id);

            // Send message in match2 to make it more recent
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user3Token);
            await _client.PostAsync($"/api/matches/{match2Id}/messages",
                JsonContent(new SendMessageRequest { MatchId = match2Id, Text = "Recent message" }));

            // Act - Get matches for user1
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            var matchesResponse = await _client.GetAsync("/api/matches");
            matchesResponse.EnsureSuccessStatusCode();
            var matches = await ParseResponse<List<MatchResponse>>(matchesResponse);

            // Assert
            Assert.Equal(2, matches.Count);
            Assert.Equal(match2Id, matches[0].MatchId); // Match with recent message should be first
            Assert.Equal(match1Id, matches[1].MatchId);
        }

        // Helper methods
        private async Task<(string token, Guid userId)> CreateAndAuthenticateUser(string email, string displayName)
        {
            var registerRequest = new RegisterRequest
            {
                Email = email,
                Password = "TestPass123!",
                DisplayName = displayName,
                BirthDate = DateTime.UtcNow.AddYears(-25),
                Gender = "male"
            };

            var registerResponse = await _client.PostAsync("/api/auth/register", JsonContent(registerRequest));
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await _client.PostAsync("/api/auth/login",
                JsonContent(new LoginRequest { Email = email, Password = "TestPass123!" }));
            loginResponse.EnsureSuccessStatusCode();

            var authResponse = await ParseResponse<AuthResponse>(loginResponse);
            return (authResponse.AccessToken, authResponse.User.Id);
        }

        private async Task<Guid> CreateMatch(string user1Token, Guid user1Id, string user2Token, Guid user2Id)
        {
            // User1 likes User2
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);
            await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user2Id, Direction = "like" }));

            // User2 likes User1 back (creates match)
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);
            var swipeResponse = await _client.PostAsync("/api/swipes",
                JsonContent(new SwipeRequest { TargetUserId = user1Id, Direction = "like" }));

            swipeResponse.EnsureSuccessStatusCode();
            var swipeResult = await ParseResponse<SwipeResponse>(swipeResponse);
            return swipeResult.MatchId!.Value;
        }

        private HttpContent JsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<T> ParseResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content)!;
        }
    }
}