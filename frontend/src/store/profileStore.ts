import { create } from 'zustand';
import { ProfileDto, PhotoDto, UpdateProfileRequest, UpdateLocationRequest } from '../types';

interface ProfileState {
  profile: ProfileDto | null;
  isLoading: boolean;
  error: string | null;

  // Actions
  setProfile: (profile: ProfileDto) => void;
  updateProfile: (updates: Partial<ProfileDto>) => void;
  addPhoto: (photo: PhotoDto) => void;
  removePhoto: (photoId: string) => void;
  setPrimaryPhoto: (photoId: string) => void;
  updateLocation: (lat: number, lng: number) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearProfile: () => void;
}

export const useProfileStore = create<ProfileState>((set) => ({
  profile: null,
  isLoading: false,
  error: null,

  setProfile: (profile: ProfileDto) => set({ profile, error: null }),

  updateProfile: (updates: Partial<ProfileDto>) =>
    set((state) => ({
      profile: state.profile ? { ...state.profile, ...updates } : null,
      error: null,
    })),

  addPhoto: (photo: PhotoDto) =>
    set((state) => {
      if (!state.profile) return state;

      const photos = [...(state.profile.photos || []), photo];
      // If this is the first photo, make it primary
      if (photos.length === 1) {
        photo.isPrimary = true;
      }

      return {
        profile: { ...state.profile, photos },
        error: null,
      };
    }),

  removePhoto: (photoId: string) =>
    set((state) => {
      if (!state.profile) return state;

      const photos = (state.profile.photos || []).filter(p => p.id !== photoId);

      // If we removed the primary photo, make the first remaining photo primary
      if (photos.length > 0 && !photos.some(p => p.isPrimary)) {
        photos[0].isPrimary = true;
      }

      return {
        profile: { ...state.profile, photos },
        error: null,
      };
    }),

  setPrimaryPhoto: (photoId: string) =>
    set((state) => {
      if (!state.profile) return state;

      const photos = (state.profile.photos || []).map(photo => ({
        ...photo,
        isPrimary: photo.id === photoId,
      }));

      return {
        profile: { ...state.profile, photos },
        error: null,
      };
    }),

  updateLocation: (lat: number, lng: number) =>
    set((state) => {
      if (!state.profile) return state;

      return {
        profile: {
          ...state.profile,
          hasLocation: true,
          locationUpdatedAt: new Date().toISOString(),
        },
        error: null,
      };
    }),

  setLoading: (isLoading: boolean) => set({ isLoading }),

  setError: (error: string | null) => set({ error }),

  clearProfile: () => set({ profile: null, error: null }),
}));