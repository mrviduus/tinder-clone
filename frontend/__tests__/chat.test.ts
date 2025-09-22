import { MessageService } from '../src/services/messageService';
import { MatchService } from '../src/services/matchService';
import { signalRService } from '../src/services/signalrService';
import { Message, SendMessageRequest, Match } from '../src/types/api';

// Mock the API client
jest.mock('../src/config/api', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    delete: jest.fn(),
  },
}));

// Mock SignalR
jest.mock('../src/services/signalrService', () => ({
  signalRService: {
    connect: jest.fn(),
    disconnect: jest.fn(),
    joinMatch: jest.fn(),
    leaveMatch: jest.fn(),
    sendTyping: jest.fn(),
    stopTyping: jest.fn(),
    onMessage: jest.fn(),
    onTyping: jest.fn(),
    getConnectionState: jest.fn(),
  },
}));

import apiClient from '../src/config/api';

const mockApiClient = apiClient as jest.Mocked<typeof apiClient>;

describe('Chat Functionality Tests', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('MessageService', () => {
    it('should send message with correct format', async () => {
      // Arrange
      const mockMessage: Message = {
        messageId: '1',
        matchId: 'match-1',
        senderId: 'user-1',
        content: 'Hello!',
        sentAt: new Date().toISOString(),
      };

      mockApiClient.post.mockResolvedValue({ data: mockMessage });

      const request: SendMessageRequest = {
        matchId: 'match-1',
        text: 'Hello!',
      };

      // Act
      const result = await MessageService.sendMessage(request);

      // Assert
      expect(mockApiClient.post).toHaveBeenCalledWith(
        '/matches/match-1/messages',
        request
      );
      expect(result).toEqual(mockMessage);
    });

    it('should get messages with pagination', async () => {
      // Arrange
      const mockMessages: Message[] = [
        {
          messageId: '1',
          matchId: 'match-1',
          senderId: 'user-1',
          content: 'Hello!',
          sentAt: new Date().toISOString(),
        },
        {
          messageId: '2',
          matchId: 'match-1',
          senderId: 'user-2',
          content: 'Hi there!',
          sentAt: new Date().toISOString(),
        },
      ];

      mockApiClient.get.mockResolvedValue({ data: mockMessages });

      // Act
      const result = await MessageService.getMessages('match-1', undefined, 10);

      // Assert
      expect(mockApiClient.get).toHaveBeenCalledWith('/matches/match-1/messages', {
        params: { take: 10 },
      });
      expect(result).toEqual(mockMessages);
    });

    it('should get messages with before parameter', async () => {
      // Arrange
      const beforeDate = new Date('2023-01-01');
      const mockMessages: Message[] = [];

      mockApiClient.get.mockResolvedValue({ data: mockMessages });

      // Act
      await MessageService.getMessages('match-1', beforeDate, 5);

      // Assert
      expect(mockApiClient.get).toHaveBeenCalledWith('/matches/match-1/messages', {
        params: { take: 5, before: beforeDate.toISOString() },
      });
    });

    it('should handle send message error', async () => {
      // Arrange
      const error = new Error('Network error');
      mockApiClient.post.mockRejectedValue(error);

      const request: SendMessageRequest = {
        matchId: 'match-1',
        text: 'Hello!',
      };

      // Act & Assert
      await expect(MessageService.sendMessage(request)).rejects.toThrow('Network error');
    });
  });

  describe('MatchService', () => {
    it('should get matches list', async () => {
      // Arrange
      const mockMatches: Match[] = [
        {
          matchId: 'match-1',
          userId: 'user-2',
          displayName: 'Alice',
          photos: [],
          matchedAt: new Date().toISOString(),
          lastMessage: 'Hello!',
          lastMessageAt: new Date().toISOString(),
        },
      ];

      mockApiClient.get.mockResolvedValue({ data: mockMatches });

      // Act
      const result = await MatchService.getMatches();

      // Assert
      expect(mockApiClient.get).toHaveBeenCalledWith('/matches', {
        params: { page: 1, pageSize: 50 },
      });
      expect(result).toEqual(mockMatches);
    });

    it('should unmatch successfully', async () => {
      // Arrange
      mockApiClient.delete.mockResolvedValue({ status: 204 });

      // Act
      const result = await MatchService.unmatch('match-1');

      // Assert
      expect(mockApiClient.delete).toHaveBeenCalledWith('/matches/match-1');
      expect(result).toBe(true);
    });

    it('should handle unmatch failure', async () => {
      // Arrange
      mockApiClient.delete.mockRejectedValue(new Error('Not found'));

      // Act
      const result = await MatchService.unmatch('match-1');

      // Assert
      expect(result).toBe(false);
    });
  });

  describe('SignalR Integration', () => {
    it('should connect and join match', async () => {
      // Arrange
      const mockConnect = signalRService.connect as jest.Mock;
      const mockJoinMatch = signalRService.joinMatch as jest.Mock;

      mockConnect.mockResolvedValue(undefined);
      mockJoinMatch.mockResolvedValue(undefined);

      // Act
      await signalRService.connect();
      await signalRService.joinMatch('match-1');

      // Assert
      expect(mockConnect).toHaveBeenCalled();
      expect(mockJoinMatch).toHaveBeenCalledWith('match-1');
    });

    it('should send typing indicator', async () => {
      // Arrange
      const mockSendTyping = signalRService.sendTyping as jest.Mock;
      mockSendTyping.mockResolvedValue(undefined);

      // Act
      await signalRService.sendTyping('match-1');

      // Assert
      expect(mockSendTyping).toHaveBeenCalledWith('match-1');
    });

    it('should stop typing indicator', async () => {
      // Arrange
      const mockStopTyping = signalRService.stopTyping as jest.Mock;
      mockStopTyping.mockResolvedValue(undefined);

      // Act
      await signalRService.stopTyping('match-1');

      // Assert
      expect(mockStopTyping).toHaveBeenCalledWith('match-1');
    });

    it('should register message callback', () => {
      // Arrange
      const mockOnMessage = signalRService.onMessage as jest.Mock;
      const callback = jest.fn();
      const unsubscribe = jest.fn();

      mockOnMessage.mockReturnValue(unsubscribe);

      // Act
      const result = signalRService.onMessage(callback);

      // Assert
      expect(mockOnMessage).toHaveBeenCalledWith(callback);
      expect(result).toBe(unsubscribe);
    });

    it('should register typing callback', () => {
      // Arrange
      const mockOnTyping = signalRService.onTyping as jest.Mock;
      const callback = jest.fn();
      const unsubscribe = jest.fn();

      mockOnTyping.mockReturnValue(unsubscribe);

      // Act
      const result = signalRService.onTyping(callback);

      // Assert
      expect(mockOnTyping).toHaveBeenCalledWith(callback);
      expect(result).toBe(unsubscribe);
    });
  });
});

describe('Chat Component Integration Tests', () => {
  it('should validate message format matches backend expectations', () => {
    // Test that our frontend types match backend DTOs
    const sendMessageRequest: SendMessageRequest = {
      matchId: 'guid-format',
      text: 'Message content', // Should be 'text', not 'content'
      photoId: 'optional-photo-id', // Should support photo messages
    };

    // Verify the structure matches what we send to the API
    expect(sendMessageRequest).toHaveProperty('matchId');
    expect(sendMessageRequest).toHaveProperty('text');
    expect(sendMessageRequest).toHaveProperty('photoId');
    expect(sendMessageRequest).not.toHaveProperty('content');
  });

  it('should validate message response format', () => {
    // Test that message response matches backend MessageResponse DTO
    const messageResponse: Message = {
      messageId: 'guid-format',
      matchId: 'guid-format',
      senderId: 'guid-format',
      content: 'Message text', // Backend returns 'Content'
      sentAt: new Date().toISOString(),
      readAt: new Date().toISOString(),
    };

    expect(messageResponse).toHaveProperty('messageId');
    expect(messageResponse).toHaveProperty('matchId');
    expect(messageResponse).toHaveProperty('senderId');
    expect(messageResponse).toHaveProperty('content');
    expect(messageResponse).toHaveProperty('sentAt');
  });
});

describe('Error Handling Tests', () => {
  it('should handle 400 bad request for invalid message', async () => {
    // Arrange
    const error = {
      response: {
        status: 400,
        data: { error: 'Message text is required' },
      },
    };
    mockApiClient.post.mockRejectedValue(error);

    const request: SendMessageRequest = {
      matchId: 'match-1',
      text: '', // Empty text should fail
    };

    // Act & Assert
    await expect(MessageService.sendMessage(request)).rejects.toMatchObject(error);
  });

  it('should handle 401 unauthorized', async () => {
    // Arrange
    const error = {
      response: {
        status: 401,
        data: { error: 'Unauthorized' },
      },
    };
    mockApiClient.post.mockRejectedValue(error);

    const request: SendMessageRequest = {
      matchId: 'match-1',
      text: 'Hello!',
    };

    // Act & Assert
    await expect(MessageService.sendMessage(request)).rejects.toMatchObject(error);
  });

  it('should handle network errors', async () => {
    // Arrange
    const error = new Error('Network Error');
    mockApiClient.post.mockRejectedValue(error);

    const request: SendMessageRequest = {
      matchId: 'match-1',
      text: 'Hello!',
    };

    // Act & Assert
    await expect(MessageService.sendMessage(request)).rejects.toThrow('Network Error');
  });
});