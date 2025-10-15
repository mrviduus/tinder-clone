import { create } from 'zustand';
import { MessageDto, TypingIndicator } from '../types';

interface ChatState {
  messages: { [matchId: string]: MessageDto[] };
  typingIndicators: { [matchId: string]: TypingIndicator };
  isLoading: boolean;
  error: string | null;
  currentMatchId: string | null;

  // Actions
  setMessages: (matchId: string, messages: MessageDto[]) => void;
  addMessage: (matchId: string, message: MessageDto) => void;
  prependMessages: (matchId: string, messages: MessageDto[]) => void;
  updateMessageStatus: (matchId: string, messageId: string, status: { deliveredAt?: Date; readAt?: Date }) => void;
  markMessagesAsRead: (matchId: string, messageIds: string[]) => void;
  setTyping: (matchId: string, userId: string, isTyping: boolean) => void;
  clearTyping: (matchId: string) => void;
  setCurrentMatch: (matchId: string | null) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearChat: (matchId: string) => void;
  clearAllChats: () => void;
}

export const useChatStore = create<ChatState>((set) => ({
  messages: {},
  typingIndicators: {},
  isLoading: false,
  error: null,
  currentMatchId: null,

  setMessages: (matchId: string, messages: MessageDto[]) =>
    set((state) => ({
      messages: {
        ...state.messages,
        [matchId]: messages,
      },
      error: null,
    })),

  addMessage: (matchId: string, message: MessageDto) =>
    set((state) => {
      const existingMessages = state.messages[matchId] || [];
      // Check if message already exists to prevent duplicates
      if (existingMessages.find(m => m.messageId === message.messageId)) {
        return state;
      }
      return {
        messages: {
          ...state.messages,
          [matchId]: [...existingMessages, message],
        },
        error: null,
      };
    }),

  prependMessages: (matchId: string, messages: MessageDto[]) =>
    set((state) => {
      const existingMessages = state.messages[matchId] || [];
      // Filter out any duplicates
      const newMessages = messages.filter(
        m => !existingMessages.find(em => em.messageId === m.messageId)
      );
      return {
        messages: {
          ...state.messages,
          [matchId]: [...newMessages, ...existingMessages],
        },
        error: null,
      };
    }),

  updateMessageStatus: (matchId: string, messageId: string, status: { deliveredAt?: Date; readAt?: Date }) =>
    set((state) => {
      const messages = state.messages[matchId];
      if (!messages) return state;

      return {
        messages: {
          ...state.messages,
          [matchId]: messages.map(m =>
            m.messageId === messageId
              ? { ...m, ...status }
              : m
          ),
        },
      };
    }),

  markMessagesAsRead: (matchId: string, messageIds: string[]) =>
    set((state) => {
      const messages = state.messages[matchId];
      if (!messages) return state;

      const readAt = new Date();
      return {
        messages: {
          ...state.messages,
          [matchId]: messages.map(m =>
            messageIds.includes(m.messageId) && !m.readAt
              ? { ...m, readAt }
              : m
          ),
        },
      };
    }),

  setTyping: (matchId: string, userId: string, isTyping: boolean) =>
    set((state) => ({
      typingIndicators: {
        ...state.typingIndicators,
        [matchId]: {
          ...state.typingIndicators[matchId],
          [userId]: isTyping,
        },
      },
    })),

  clearTyping: (matchId: string) =>
    set((state) => {
      const { [matchId]: _, ...rest } = state.typingIndicators;
      return {
        typingIndicators: rest,
      };
    }),

  setCurrentMatch: (matchId: string | null) =>
    set({ currentMatchId: matchId }),

  setLoading: (isLoading: boolean) => set({ isLoading }),

  setError: (error: string | null) => set({ error }),

  clearChat: (matchId: string) =>
    set((state) => {
      const { [matchId]: _, ...restMessages } = state.messages;
      const { [matchId]: __, ...restTyping } = state.typingIndicators;
      return {
        messages: restMessages,
        typingIndicators: restTyping,
      };
    }),

  clearAllChats: () =>
    set({
      messages: {},
      typingIndicators: {},
      currentMatchId: null,
      error: null,
    }),
}));

// Helper selectors
export const getMessagesForMatch = (matchId: string) => {
  const state = useChatStore.getState();
  return state.messages[matchId] || [];
};

export const getUnreadCount = (matchId: string, currentUserId: string) => {
  const messages = getMessagesForMatch(matchId);
  return messages.filter(m => m.senderId !== currentUserId && !m.readAt).length;
};

export const getLastMessage = (matchId: string) => {
  const messages = getMessagesForMatch(matchId);
  return messages[messages.length - 1];
};

export const isUserTyping = (matchId: string, userId: string) => {
  const state = useChatStore.getState();
  return state.typingIndicators[matchId]?.[userId] || false;
};