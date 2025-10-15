import { create } from 'zustand';
import { MatchDto } from '../types';

interface MatchState {
  matches: MatchDto[];
  isLoading: boolean;
  error: string | null;
  currentPage: number;
  hasMore: boolean;

  // Actions
  setMatches: (matches: MatchDto[]) => void;
  addMatches: (matches: MatchDto[]) => void;
  removeMatch: (matchId: string) => void;
  updateLastMessage: (matchId: string, message: string, unreadCount: number) => void;
  incrementUnread: (matchId: string) => void;
  clearUnread: (matchId: string) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  resetMatches: () => void;
  incrementPage: () => void;
  setHasMore: (hasMore: boolean) => void;
}

export const useMatchStore = create<MatchState>((set) => ({
  matches: [],
  isLoading: false,
  error: null,
  currentPage: 1,
  hasMore: true,

  setMatches: (matches: MatchDto[]) =>
    set({
      matches,
      error: null,
    }),

  addMatches: (newMatches: MatchDto[]) =>
    set((state) => ({
      matches: [...state.matches, ...newMatches],
      error: null,
    })),

  removeMatch: (matchId: string) =>
    set((state) => ({
      matches: state.matches.filter(m => m.matchId !== matchId),
    })),

  updateLastMessage: (matchId: string, message: string, unreadCount: number) =>
    set((state) => ({
      matches: state.matches.map(match =>
        match.matchId === matchId
          ? {
              ...match,
              lastMessagePreview: message,
              unreadCount: unreadCount,
            }
          : match
      ),
    })),

  incrementUnread: (matchId: string) =>
    set((state) => ({
      matches: state.matches.map(match =>
        match.matchId === matchId
          ? { ...match, unreadCount: match.unreadCount + 1 }
          : match
      ),
    })),

  clearUnread: (matchId: string) =>
    set((state) => ({
      matches: state.matches.map(match =>
        match.matchId === matchId
          ? { ...match, unreadCount: 0 }
          : match
      ),
    })),

  setLoading: (isLoading: boolean) => set({ isLoading }),

  setError: (error: string | null) => set({ error }),

  resetMatches: () =>
    set({
      matches: [],
      error: null,
      currentPage: 1,
      hasMore: true,
    }),

  incrementPage: () =>
    set((state) => ({
      currentPage: state.currentPage + 1,
    })),

  setHasMore: (hasMore: boolean) => set({ hasMore }),
}));

// Helper selectors
export const getMatchById = (matchId: string) => {
  const state = useMatchStore.getState();
  return state.matches.find(m => m.matchId === matchId);
};

export const getTotalUnreadCount = () => {
  const state = useMatchStore.getState();
  return state.matches.reduce((total, match) => total + match.unreadCount, 0);
};

export const getRecentMatches = (limit: number = 5) => {
  const state = useMatchStore.getState();
  return state.matches
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    .slice(0, limit);
};