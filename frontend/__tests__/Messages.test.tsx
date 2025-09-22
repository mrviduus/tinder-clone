import React from 'react';
import { render, fireEvent, waitFor, act } from '@testing-library/react-native';
import { Alert } from 'react-native';
import Messages from '../screens/Messages';
import { MessageService } from '../src/services/messageService';
import { MatchService } from '../src/services/matchService';
import { signalRService } from '../src/services/signalrService';
import { useAuthStore } from '../src/store/authStore';

// Mock navigation
const mockGoBack = jest.fn();
const mockNavigate = jest.fn();
const mockRoute = {
  params: {
    matchId: 'test-match-id',
    matchName: 'Test User',
  },
};

jest.mock('@react-navigation/native', () => ({
  useNavigation: () => ({
    goBack: mockGoBack,
    navigate: mockNavigate,
  }),
  useRoute: () => mockRoute,
}));

// Mock services
jest.mock('../src/services/messageService');
jest.mock('../src/services/matchService');
jest.mock('../src/services/signalrService');
jest.mock('../src/store/authStore');

// Mock Alert
jest.spyOn(Alert, 'alert');

describe('Messages Component', () => {
  const mockUser = {
    id: 'current-user-id',
    email: 'test@test.com',
    displayName: 'Current User',
  };

  const mockMessages = [
    {
      messageId: '1',
      matchId: 'test-match-id',
      senderId: 'current-user-id',
      content: 'Hello!',
      sentAt: new Date('2024-01-01T10:00:00').toISOString(),
    },
    {
      messageId: '2',
      matchId: 'test-match-id',
      senderId: 'other-user-id',
      content: 'Hi there!',
      sentAt: new Date('2024-01-01T10:01:00').toISOString(),
    },
  ];

  beforeEach(() => {
    jest.clearAllMocks();

    // Setup auth store mock
    (useAuthStore as jest.Mock).mockReturnValue({
      user: mockUser,
    });

    // Setup MessageService mocks
    (MessageService.getMessages as jest.Mock).mockResolvedValue(mockMessages);
    (MessageService.sendMessage as jest.Mock).mockResolvedValue({
      messageId: '3',
      matchId: 'test-match-id',
      senderId: 'current-user-id',
      content: 'New message',
      sentAt: new Date().toISOString(),
    });

    // Setup MatchService mocks
    (MatchService.unmatch as jest.Mock).mockResolvedValue(true);

    // Setup SignalR mocks
    (signalRService.connect as jest.Mock).mockResolvedValue(undefined);
    (signalRService.joinMatch as jest.Mock).mockResolvedValue(undefined);
    (signalRService.leaveMatch as jest.Mock).mockResolvedValue(undefined);
    (signalRService.sendTyping as jest.Mock).mockResolvedValue(undefined);
    (signalRService.stopTyping as jest.Mock).mockResolvedValue(undefined);
    (signalRService.onMessage as jest.Mock).mockReturnValue(() => {});
    (signalRService.onTyping as jest.Mock).mockReturnValue(() => {});
  });

  describe('Component Rendering', () => {
    it('should render loading state initially', () => {
      const { getByText } = render(<Messages />);
      expect(getByText('Loading messages...')).toBeTruthy();
    });

    it('should render header with match name', async () => {
      const { getByText } = render(<Messages />);
      await waitFor(() => {
        expect(getByText('Test User')).toBeTruthy();
      });
    });

    it('should render input field and send button', async () => {
      const { getByPlaceholderText, getByTestId } = render(<Messages />);
      await waitFor(() => {
        expect(getByPlaceholderText('Type a message...')).toBeTruthy();
      });
    });

    it('should render messages after loading', async () => {
      const { getByText } = render(<Messages />);
      await waitFor(() => {
        expect(getByText('Hello!')).toBeTruthy();
        expect(getByText('Hi there!')).toBeTruthy();
      });
    });

    it('should show empty state when no messages', async () => {
      (MessageService.getMessages as jest.Mock).mockResolvedValue([]);
      const { getByText } = render(<Messages />);
      await waitFor(() => {
        expect(getByText('Start a conversation with Test User')).toBeTruthy();
      });
    });
  });

  describe('Message Sending', () => {
    it('should send message when input is not empty', async () => {
      const { getByPlaceholderText, getByTestId } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, 'Test message');
      });

      // Find send button and press it
      await act(async () => {
        const sendButton = getByTestId('send-button');
        fireEvent.press(sendButton);
      });

      expect(MessageService.sendMessage).toHaveBeenCalledWith({
        matchId: 'test-match-id',
        text: 'Test message',
      });
    });

    it('should not send empty message', async () => {
      const { getByPlaceholderText, getByTestId } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, '   '); // Only spaces
      });

      const sendButton = getByTestId('send-button');
      expect(sendButton.props.disabled).toBe(true);

      fireEvent.press(sendButton);
      expect(MessageService.sendMessage).not.toHaveBeenCalled();
    });

    it('should clear input after sending message', async () => {
      const { getByPlaceholderText, getByTestId } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, 'Test message');
      });

      await act(async () => {
        const sendButton = getByTestId('send-button');
        fireEvent.press(sendButton);
      });

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        expect(input.props.value).toBe('');
      });
    });

    it('should show error alert when message send fails', async () => {
      (MessageService.sendMessage as jest.Mock).mockRejectedValue(new Error('Send failed'));

      const { getByPlaceholderText, getByTestId } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, 'Test message');
      });

      await act(async () => {
        const sendButton = getByTestId('send-button');
        fireEvent.press(sendButton);
      });

      await waitFor(() => {
        expect(Alert.alert).toHaveBeenCalledWith('Error', 'Failed to send message');
      });
    });
  });

  describe('Typing Indicators', () => {
    it('should send typing indicator when typing', async () => {
      const { getByPlaceholderText } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, 'T');
      });

      expect(signalRService.sendTyping).toHaveBeenCalledWith('test-match-id');
    });

    it('should stop typing indicator when input is cleared', async () => {
      const { getByPlaceholderText } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        fireEvent.changeText(input, 'Test');
        fireEvent.changeText(input, '');
      });

      expect(signalRService.stopTyping).toHaveBeenCalledWith('test-match-id');
    });

    it('should display typing indicator when other user is typing', async () => {
      let typingCallback: any;
      (signalRService.onTyping as jest.Mock).mockImplementation((callback) => {
        typingCallback = callback;
        return () => {};
      });

      const { getByText } = render(<Messages />);

      await waitFor(() => {
        expect(signalRService.onTyping).toHaveBeenCalled();
      });

      act(() => {
        typingCallback({ userId: 'other-user-id', isTyping: true });
      });

      await waitFor(() => {
        expect(getByText('Test User is typing...')).toBeTruthy();
      });
    });
  });

  describe('Unmatch Functionality', () => {
    it('should show confirmation dialog when unmatch is pressed', async () => {
      const { getByTestId } = render(<Messages />);

      await waitFor(() => {
        const unmatchButton = getByTestId('unmatch-button');
        fireEvent.press(unmatchButton);
      });

      expect(Alert.alert).toHaveBeenCalledWith(
        'Unmatch',
        expect.stringContaining('Are you sure you want to unmatch with Test User'),
        expect.any(Array)
      );
    });

    it('should call unmatch service when confirmed', async () => {
      const { getByTestId } = render(<Messages />);

      await waitFor(() => {
        const unmatchButton = getByTestId('unmatch-button');
        fireEvent.press(unmatchButton);
      });

      // Get the onPress handler for the "Unmatch" button from Alert
      const alertCall = (Alert.alert as jest.Mock).mock.calls[0];
      const buttons = alertCall[2];
      const unmatchButton = buttons.find((b: any) => b.text === 'Unmatch');

      await act(async () => {
        await unmatchButton.onPress();
      });

      expect(MatchService.unmatch).toHaveBeenCalledWith('test-match-id');
      expect(mockGoBack).toHaveBeenCalled();
    });
  });

  describe('SignalR Connection', () => {
    it('should connect to SignalR on mount', async () => {
      render(<Messages />);

      await waitFor(() => {
        expect(signalRService.connect).toHaveBeenCalled();
        expect(signalRService.joinMatch).toHaveBeenCalledWith('test-match-id');
      });
    });

    it('should leave match on unmount', async () => {
      const { unmount } = render(<Messages />);

      await waitFor(() => {
        expect(signalRService.connect).toHaveBeenCalled();
      });

      unmount();

      expect(signalRService.leaveMatch).toHaveBeenCalledWith('test-match-id');
    });

    it('should add received messages to the list', async () => {
      let messageCallback: any;
      (signalRService.onMessage as jest.Mock).mockImplementation((callback) => {
        messageCallback = callback;
        return () => {};
      });

      const { getByText } = render(<Messages />);

      await waitFor(() => {
        expect(signalRService.onMessage).toHaveBeenCalled();
      });

      act(() => {
        messageCallback({
          messageId: '4',
          senderId: 'other-user-id',
          content: 'New incoming message',
          sentAt: new Date().toISOString(),
        });
      });

      await waitFor(() => {
        expect(getByText('New incoming message')).toBeTruthy();
      });
    });
  });

  describe('Input Field Visibility', () => {
    it('should keep input field visible with many messages', async () => {
      const manyMessages = Array.from({ length: 50 }, (_, i) => ({
        messageId: `${i}`,
        matchId: 'test-match-id',
        senderId: i % 2 === 0 ? 'current-user-id' : 'other-user-id',
        content: `Message ${i}`,
        sentAt: new Date().toISOString(),
      }));

      (MessageService.getMessages as jest.Mock).mockResolvedValue(manyMessages);

      const { getByPlaceholderText } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        expect(input).toBeTruthy();
        expect(input.props.style).toBeDefined();
      });
    });

    it('should have fixed input area that does not scroll', async () => {
      const { getByPlaceholderText, getByTestId } = render(<Messages />);

      await waitFor(() => {
        const input = getByPlaceholderText('Type a message...');
        const inputContainer = input.parent?.parent;

        // Check that input container has proper styling
        expect(inputContainer?.props.style).toMatchObject(
          expect.objectContaining({
            flexDirection: 'row',
            padding: 16,
            backgroundColor: '#FFFFFF',
          })
        );
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle message loading error', async () => {
      (MessageService.getMessages as jest.Mock).mockRejectedValue(new Error('Load failed'));

      render(<Messages />);

      await waitFor(() => {
        expect(Alert.alert).toHaveBeenCalledWith('Error', 'Failed to load messages');
      });
    });

    it('should handle SignalR connection error gracefully', async () => {
      (signalRService.connect as jest.Mock).mockRejectedValue(new Error('Connection failed'));

      const consoleSpy = jest.spyOn(console, 'error').mockImplementation();

      render(<Messages />);

      await waitFor(() => {
        expect(consoleSpy).toHaveBeenCalledWith('SignalR setup error:', expect.any(Error));
      });

      consoleSpy.mockRestore();
    });

    it('should navigate back if no matchId provided', async () => {
      mockRoute.params = undefined;

      render(<Messages />);

      await waitFor(() => {
        expect(mockGoBack).toHaveBeenCalled();
      });
    });
  });
});