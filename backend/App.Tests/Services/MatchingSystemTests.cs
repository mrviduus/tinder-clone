using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using App.Data;
using App.Domain;
using App.Services;
using App.DTOs;

namespace App.Tests.Services
{
    public class MatchingSystemTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly SwipeService _swipeService;
        private readonly MatchService _matchService;
        private readonly MessageService _messageService;
        private readonly ProfileService _profileService;

        private readonly User _user1;
        private readonly User _user2;
        private readonly User _user3;

        public MatchingSystemTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _profileService = new ProfileService(_context);
            _matchService = new MatchService(_context, _profileService);
            _swipeService = new SwipeService(_context);
            _messageService = new MessageService(_context, _matchService);

            // Setup test users
            _user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@test.com",
                Profile = new Profile
                {
                    DisplayName = "User 1",
                    Bio = "Test bio 1",
                    BirthDate = DateTime.UtcNow.AddYears(-25)
                }
            };

            _user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@test.com",
                Profile = new Profile
                {
                    DisplayName = "User 2",
                    Bio = "Test bio 2",
                    BirthDate = DateTime.UtcNow.AddYears(-26)
                }
            };

            _user3 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user3@test.com",
                Profile = new Profile
                {
                    DisplayName = "User 3",
                    Bio = "Test bio 3",
                    BirthDate = DateTime.UtcNow.AddYears(-24)
                }
            };

            _context.Users.AddRange(_user1, _user2, _user3);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ProcessSwipe_SingleLike_NoMatch()
        {
            // Arrange
            var request = new SwipeRequest
            {
                TargetUserId = _user2.Id,
                Direction = "like"
            };

            // Act
            var result = await _swipeService.ProcessSwipeAsync(_user1.Id, request);

            // Assert
            Assert.False(result.Matched);
            Assert.Null(result.MatchId);

            var swipe = await _context.Swipes
                .FirstOrDefaultAsync(s => s.SwiperId == _user1.Id && s.TargetId == _user2.Id);
            Assert.NotNull(swipe);
            Assert.Equal(SwipeDirection.Like, swipe.Direction);
        }

        [Fact]
        public async Task ProcessSwipe_MutualLike_CreatesMatch()
        {
            // Arrange - User1 likes User2
            var request1 = new SwipeRequest
            {
                TargetUserId = _user2.Id,
                Direction = "like"
            };
            await _swipeService.ProcessSwipeAsync(_user1.Id, request1);

            // Act - User2 likes User1 back
            var request2 = new SwipeRequest
            {
                TargetUserId = _user1.Id,
                Direction = "like"
            };
            var result = await _swipeService.ProcessSwipeAsync(_user2.Id, request2);

            // Assert
            Assert.True(result.Matched);
            Assert.NotNull(result.MatchId);

            var match = await _context.Matches
                .FirstOrDefaultAsync(m => m.Id == result.MatchId);
            Assert.NotNull(match);
            Assert.True(
                (match.UserAId == _user1.Id && match.UserBId == _user2.Id) ||
                (match.UserAId == _user2.Id && match.UserBId == _user1.Id)
            );
        }

        [Fact]
        public async Task ProcessSwipe_Pass_NoMatch()
        {
            // Arrange
            var request = new SwipeRequest
            {
                TargetUserId = _user2.Id,
                Direction = "pass"
            };

            // Act
            var result = await _swipeService.ProcessSwipeAsync(_user1.Id, request);

            // Assert
            Assert.False(result.Matched);
            Assert.Null(result.MatchId);

            var swipe = await _context.Swipes
                .FirstOrDefaultAsync(s => s.SwiperId == _user1.Id && s.TargetId == _user2.Id);
            Assert.NotNull(swipe);
            Assert.Equal(SwipeDirection.Pass, swipe.Direction);
        }

        [Fact]
        public async Task ProcessSwipe_DuplicateSwipe_ReturnsExistingResult()
        {
            // Arrange - First swipe
            var request = new SwipeRequest
            {
                TargetUserId = _user2.Id,
                Direction = "like"
            };
            var firstResult = await _swipeService.ProcessSwipeAsync(_user1.Id, request);

            // Act - Duplicate swipe
            var duplicateResult = await _swipeService.ProcessSwipeAsync(_user1.Id, request);

            // Assert
            Assert.Equal(firstResult.Matched, duplicateResult.Matched);
            Assert.Equal(firstResult.MatchId, duplicateResult.MatchId);

            var swipeCount = await _context.Swipes
                .CountAsync(s => s.SwiperId == _user1.Id && s.TargetId == _user2.Id);
            Assert.Equal(1, swipeCount); // Should not create duplicate swipes
        }

        [Fact]
        public async Task GetMatches_ReturnsAllMatches()
        {
            // Arrange - Create two matches
            await CreateMatch(_user1.Id, _user2.Id);
            await CreateMatch(_user1.Id, _user3.Id);

            // Act
            var matches = await _matchService.GetMatchesAsync(_user1.Id);

            // Assert
            Assert.Equal(2, matches.Count);
            Assert.All(matches, m => Assert.NotNull(m.Counterpart));
        }

        [Fact]
        public async Task SendMessage_ValidMatch_Success()
        {
            // Arrange - Create a match
            var matchId = await CreateMatch(_user1.Id, _user2.Id);
            var request = new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hello!",
                PhotoId = null
            };

            // Act
            var message = await _messageService.SendMessageAsync(_user1.Id, request);

            // Assert
            Assert.NotNull(message);
            Assert.Equal("Hello!", message.Content);
            Assert.Equal(_user1.Id, message.SenderId);
            Assert.Equal(matchId, message.MatchId);
        }

        [Fact]
        public async Task SendMessage_NotInMatch_ReturnsNull()
        {
            // Arrange - Create a match between user1 and user2, but user3 tries to send
            var matchId = await CreateMatch(_user1.Id, _user2.Id);
            var request = new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hello!",
                PhotoId = null
            };

            // Act
            var message = await _messageService.SendMessageAsync(_user3.Id, request);

            // Assert
            Assert.Null(message);
        }

        [Fact]
        public async Task GetMessages_ValidMatch_ReturnsMessages()
        {
            // Arrange - Create match and send messages
            var matchId = await CreateMatch(_user1.Id, _user2.Id);

            await _messageService.SendMessageAsync(_user1.Id, new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hello!"
            });

            await _messageService.SendMessageAsync(_user2.Id, new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hi there!"
            });

            // Act
            var messages = await _messageService.GetMessagesAsync(matchId, _user1.Id);

            // Assert
            Assert.Equal(2, messages.Count);
            Assert.Equal("Hello!", messages[0].Content);
            Assert.Equal("Hi there!", messages[1].Content);
        }

        [Fact]
        public async Task MarkMessagesAsRead_ValidMatch_Success()
        {
            // Arrange - Create match and send message from user1 to user2
            var matchId = await CreateMatch(_user1.Id, _user2.Id);

            var sentMessage = await _messageService.SendMessageAsync(_user1.Id, new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hello!"
            });

            var request = new MarkReadRequest
            {
                MatchId = matchId,
                MessageIds = new List<Guid> { sentMessage!.Id }
            };

            // Act
            var success = await _messageService.MarkMessagesAsReadAsync(_user2.Id, request);

            // Assert
            Assert.True(success);

            var message = await _context.Messages.FindAsync(sentMessage.Id);
            Assert.NotNull(message!.ReadAt);
        }

        [Fact]
        public async Task Unmatch_ValidMatch_DeletesEverything()
        {
            // Arrange - Create match with messages
            var matchId = await CreateMatch(_user1.Id, _user2.Id);

            await _messageService.SendMessageAsync(_user1.Id, new SendMessageRequest
            {
                MatchId = matchId,
                Text = "Hello!"
            });

            // Act
            var success = await _matchService.UnmatchAsync(matchId, _user1.Id);

            // Assert
            Assert.True(success);

            // Verify match is deleted
            var match = await _context.Matches.FindAsync(matchId);
            Assert.Null(match);

            // Verify messages are deleted
            var messages = await _context.Messages
                .Where(m => m.MatchId == matchId)
                .ToListAsync();
            Assert.Empty(messages);

            // Verify swipes are deleted
            var swipes = await _context.Swipes
                .Where(s => (s.SwiperId == _user1.Id && s.TargetId == _user2.Id) ||
                           (s.SwiperId == _user2.Id && s.TargetId == _user1.Id))
                .ToListAsync();
            Assert.Empty(swipes);
        }

        [Fact]
        public async Task Unmatch_NotInMatch_ReturnsFalse()
        {
            // Arrange - Create match between user1 and user2
            var matchId = await CreateMatch(_user1.Id, _user2.Id);

            // Act - User3 tries to unmatch
            var success = await _matchService.UnmatchAsync(matchId, _user3.Id);

            // Assert
            Assert.False(success);

            // Verify match still exists
            var match = await _context.Matches.FindAsync(matchId);
            Assert.NotNull(match);
        }

        [Fact]
        public async Task IsUserInMatch_ValidUser_ReturnsTrue()
        {
            // Arrange
            var matchId = await CreateMatch(_user1.Id, _user2.Id);

            // Act
            var user1InMatch = await _matchService.IsUserInMatchAsync(matchId, _user1.Id);
            var user2InMatch = await _matchService.IsUserInMatchAsync(matchId, _user2.Id);
            var user3InMatch = await _matchService.IsUserInMatchAsync(matchId, _user3.Id);

            // Assert
            Assert.True(user1InMatch);
            Assert.True(user2InMatch);
            Assert.False(user3InMatch);
        }

        // Helper method to create a match
        private async Task<Guid> CreateMatch(Guid userAId, Guid userBId)
        {
            // Create mutual likes
            await _swipeService.ProcessSwipeAsync(userAId, new SwipeRequest
            {
                TargetUserId = userBId,
                Direction = "like"
            });

            var result = await _swipeService.ProcessSwipeAsync(userBId, new SwipeRequest
            {
                TargetUserId = userAId,
                Direction = "like"
            });

            return result.MatchId!.Value;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}