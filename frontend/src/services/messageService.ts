import { apiClient } from '../config/api';
import { MessageDto, SendMessageRequest, MarkReadRequest } from '../types';
import { useChatStore } from '../store/chatStore';

export class MessageService {
  static async getMessages(
    matchId: string,
    before?: Date,
    take: number = 30
  ): Promise<MessageDto[]> {
    try {
      const params: any = { take };
      if (before) {
        params.before = before.toISOString();
      }

      const response = await apiClient.get<MessageDto[]>(`/message/${matchId}`, {
        params,
      });

      const messages = response.data;

      // Update store
      if (!before) {
        // Initial load
        useChatStore.getState().setMessages(matchId, messages);
      } else {
        // Loading more (older messages)
        useChatStore.getState().prependMessages(matchId, messages);
      }

      return messages;
    } catch (error: any) {
      console.error('Failed to load messages:', error);
      throw error;
    }
  }

  static async sendMessage(request: SendMessageRequest): Promise<MessageDto> {
    try {
      const response = await apiClient.post<MessageDto>('/message', request);
      const message = response.data;

      // Add to store optimistically (will be updated via SignalR)
      useChatStore.getState().addMessage(request.matchId, message);

      return message;
    } catch (error: any) {
      console.error('Failed to send message:', error);
      throw error;
    }
  }

  static async markAsRead(matchId: string, messageIds: string[]): Promise<void> {
    try {
      const request: MarkReadRequest = {
        matchId,
        messageIds,
      };

      await apiClient.post('/message/read', request);

      // Update store
      useChatStore.getState().markMessagesAsRead(matchId, messageIds);
    } catch (error: any) {
      console.error('Failed to mark messages as read:', error);
    }
  }

  static async loadChatHistory(matchId: string): Promise<MessageDto[]> {
    useChatStore.getState().setLoading(true);

    try {
      const messages = await this.getMessages(matchId);
      return messages;
    } catch (error: any) {
      useChatStore.getState().setError(error.message);
      throw error;
    } finally {
      useChatStore.getState().setLoading(false);
    }
  }

  static async loadMoreMessages(matchId: string): Promise<MessageDto[]> {
    const existingMessages = useChatStore.getState().messages[matchId] || [];

    if (existingMessages.length === 0) {
      return this.loadChatHistory(matchId);
    }

    const oldestMessage = existingMessages[0];
    const before = new Date(oldestMessage.sentAt);

    try {
      const messages = await this.getMessages(matchId, before);
      return messages;
    } catch (error: any) {
      console.error('Failed to load more messages:', error);
      return [];
    }
  }
}