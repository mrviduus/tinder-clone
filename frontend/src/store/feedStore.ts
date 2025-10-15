import { create } from 'zustand';
import { FeedProfile, SwipeDirection } from '../types';

interface FeedState {
  profiles: FeedProfile[];
  currentIndex: number;
  isLoading: boolean;
  error: string | null;
  hasMore: boolean;
  currentPage: number;

  // Actions
  setProfiles: (profiles: FeedProfile[]) => void;
  addProfiles: (profiles: FeedProfile[]) => void;
  removeCurrentProfile: () => void;
  incrementIndex: () => void;
  resetFeed: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  setHasMore: (hasMore: boolean) => void;
  incrementPage: () => void;
}

export const useFeedStore = create<FeedState>((set, get) => ({
  profiles: [],
  currentIndex: 0,
  isLoading: false,
  error: null,
  hasMore: true,
  currentPage: 1,

  setProfiles: (profiles: FeedProfile[]) =>
    set({
      profiles,
      currentIndex: 0,
      error: null,
    }),

  addProfiles: (newProfiles: FeedProfile[]) =>
    set((state) => ({
      profiles: [...state.profiles, ...newProfiles],
      error: null,
    })),

  removeCurrentProfile: () =>
    set((state) => {
      const newProfiles = [...state.profiles];
      newProfiles.splice(state.currentIndex, 1);

      return {
        profiles: newProfiles,
        // Don't increment index as the next card will now be at the same index
        currentIndex: Math.min(state.currentIndex, newProfiles.length - 1),
      };
    }),

  incrementIndex: () =>
    set((state) => ({
      currentIndex: Math.min(state.currentIndex + 1, state.profiles.length - 1),
    })),

  resetFeed: () =>
    set({
      profiles: [],
      currentIndex: 0,
      error: null,
      hasMore: true,
      currentPage: 1,
    }),

  setLoading: (isLoading: boolean) => set({ isLoading }),

  setError: (error: string | null) => set({ error }),

  setHasMore: (hasMore: boolean) => set({ hasMore }),

  incrementPage: () =>
    set((state) => ({
      currentPage: state.currentPage + 1,
    })),
}));

// Helper selectors
export const getCurrentProfile = () => {
  const state = useFeedStore.getState();
  return state.profiles[state.currentIndex];
};

export const getQueuedProfiles = () => {
  const state = useFeedStore.getState();
  return state.profiles.slice(state.currentIndex);
};

export const needsMoreProfiles = () => {
  const state = useFeedStore.getState();
  const remainingProfiles = state.profiles.length - state.currentIndex;
  return remainingProfiles <= 3 && state.hasMore && !state.isLoading;
};