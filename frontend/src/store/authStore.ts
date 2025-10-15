import { create } from 'zustand';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { AuthState, AuthResponse } from '../types';

const AUTH_STORAGE_KEY = '@auth';

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,

  setAuth: async (response: AuthResponse) => {
    // Store tokens in AsyncStorage for persistence
    try {
      await AsyncStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        user: response.user,
      }));
    } catch (error) {
      console.error('Failed to store auth data:', error);
    }

    set({
      user: response.user,
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      isAuthenticated: true,
      error: null,
    });
  },

  clearAuth: async () => {
    // Remove tokens from AsyncStorage
    try {
      await AsyncStorage.removeItem(AUTH_STORAGE_KEY);
    } catch (error) {
      console.error('Failed to clear auth data:', error);
    }

    set({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      error: null,
    });
  },

  setLoading: (loading: boolean) => set({ isLoading: loading }),

  setError: (error: string | null) => set({ error }),
}));

// Initialize auth state from storage
export const initializeAuth = async () => {
  const { setAuth, setLoading } = useAuthStore.getState();
  setLoading(true);

  try {
    const authData = await AsyncStorage.getItem(AUTH_STORAGE_KEY);
    if (authData) {
      const parsed = JSON.parse(authData);
      if (parsed.accessToken && parsed.refreshToken && parsed.user) {
        await setAuth({
          accessToken: parsed.accessToken,
          refreshToken: parsed.refreshToken,
          expiresIn: 900, // Default 15 minutes
          user: parsed.user,
        });
        return true;
      }
    }
  } catch (error) {
    console.error('Failed to initialize auth:', error);
  } finally {
    setLoading(false);
  }

  return false;
};