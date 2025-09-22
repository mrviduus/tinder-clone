using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Controllers;
using App.DTOs;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Tests;

public class MessageControllerTests
{
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly Mock<IMatchService> _matchServiceMock;
    private readonly Mock<IHubContext<ChatHub>> _hubContextMock;
    private readonly Mock<ILogger<MessageController>> _loggerMock;
    private readonly MessageController _controller;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _matchId = Guid.NewGuid();

    public MessageControllerTests()
    {
        _messageServiceMock = new Mock<IMessageService>();
        _matchServiceMock = new Mock<IMatchService>();
        _hubContextMock = new Mock<IHubContext<ChatHub>>();
        _loggerMock = new Mock<ILogger<MessageController>>();

        _controller = new MessageController(
            _messageServiceMock.Object,
            _matchServiceMock.Object,
            _hubContextMock.Object,
            _loggerMock.Object
        );

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetMessages_ReturnsOkResult_WithMessages()
    {
        // Arrange
        var messages = new List<MessageResponse>
        {
            new MessageResponse
            {
                Id = Guid.NewGuid(),
                MatchId = _matchId,
                SenderId = _userId,
                Content = "Test message",
                SentAt = DateTime.UtcNow
            }
        };

        _messageServiceMock.Setup(x => x.GetMessagesAsync(_matchId, _userId))
            .ReturnsAsync(messages);

        // Act
        var result = await _controller.GetMessages(_matchId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedMessages = Assert.IsAssignableFrom<IEnumerable<MessageResponse>>(okResult.Value);
        Assert.Single(returnedMessages);
    }

    [Fact]
    public async Task SendMessage_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            MatchId = _matchId,
            Text = "Test message"
        };

        var match = new Match { Id = _matchId, User1Id = _userId, User2Id = Guid.NewGuid() };
        var message = new MessageResponse
        {
            Id = Guid.NewGuid(),
            MatchId = _matchId,
            SenderId = _userId,
            Content = request.Text,
            SentAt = DateTime.UtcNow
        };

        _matchServiceMock.Setup(x => x.GetMatchAsync(_matchId))
            .ReturnsAsync(match);
        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(true);
        _messageServiceMock.Setup(x => x.SendMessageAsync(_matchId, _userId, request.Text, null))
            .ReturnsAsync(message);

        var mockClients = new Mock<IHubClients>();
        var mockGroupManager = new Mock<IClientProxy>();
        _hubContextMock.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroupManager.Object);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedMessage = Assert.IsType<MessageResponse>(okResult.Value);
        Assert.Equal(message.Id, returnedMessage.Id);
    }

    [Fact]
    public async Task SendMessage_WithEmptyText_ReturnsBadRequest()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            MatchId = _matchId,
            Text = ""
        };

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendMessage_UserNotInMatch_ReturnsUnauthorized()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            MatchId = _matchId,
            Text = "Test message"
        };

        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task MarkMessagesAsRead_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new MarkReadRequest
        {
            MatchId = _matchId,
            MessageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(true);
        _messageServiceMock.Setup(x => x.MarkMessagesAsReadAsync(request.MessageIds, _userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkMessagesAsRead(request);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task MarkMessagesAsRead_UserNotInMatch_ReturnsUnauthorized()
    {
        // Arrange
        var request = new MarkReadRequest
        {
            MatchId = _matchId,
            MessageIds = new List<Guid> { Guid.NewGuid() }
        };

        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkMessagesAsRead(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task SendMessage_CallsSignalRHub()
    {
        // Arrange
        var request = new SendMessageRequest
        {
            MatchId = _matchId,
            Text = "Test message"
        };

        var match = new Match { Id = _matchId, User1Id = _userId, User2Id = Guid.NewGuid() };
        var message = new MessageResponse
        {
            Id = Guid.NewGuid(),
            MatchId = _matchId,
            SenderId = _userId,
            Content = request.Text,
            SentAt = DateTime.UtcNow
        };

        _matchServiceMock.Setup(x => x.GetMatchAsync(_matchId))
            .ReturnsAsync(match);
        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(true);
        _messageServiceMock.Setup(x => x.SendMessageAsync(_matchId, _userId, request.Text, null))
            .ReturnsAsync(message);

        var mockClients = new Mock<IHubClients>();
        var mockGroupManager = new Mock<IClientProxy>();
        _hubContextMock.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroupManager.Object);

        // Act
        await _controller.SendMessage(request);

        // Assert
        mockClients.Verify(x => x.Group($"match_{_matchId}"), Times.Once);
        mockGroupManager.Verify(x => x.SendAsync("ReceiveMessage", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task GetMessages_HandlesServiceException()
    {
        // Arrange
        _messageServiceMock.Setup(x => x.GetMessagesAsync(_matchId, _userId))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetMessages(_matchId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ValidatesPhotoId()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        var request = new SendMessageRequest
        {
            MatchId = _matchId,
            PhotoId = photoId
        };

        var match = new Match { Id = _matchId, User1Id = _userId, User2Id = Guid.NewGuid() };
        var message = new MessageResponse
        {
            Id = Guid.NewGuid(),
            MatchId = _matchId,
            SenderId = _userId,
            ImagePhotoId = photoId,
            SentAt = DateTime.UtcNow
        };

        _matchServiceMock.Setup(x => x.GetMatchAsync(_matchId))
            .ReturnsAsync(match);
        _matchServiceMock.Setup(x => x.IsUserInMatchAsync(_userId, _matchId))
            .ReturnsAsync(true);
        _messageServiceMock.Setup(x => x.SendMessageAsync(_matchId, _userId, null, photoId))
            .ReturnsAsync(message);

        var mockClients = new Mock<IHubClients>();
        var mockGroupManager = new Mock<IClientProxy>();
        _hubContextMock.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroupManager.Object);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedMessage = Assert.IsType<MessageResponse>(okResult.Value);
        Assert.Equal(photoId, returnedMessage.ImagePhotoId);
    }
}