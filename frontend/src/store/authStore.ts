import { create } from 'zustand';
import { User, AuthResponse } from '../types/api';

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  setAuth: (auth: AuthResponse, user: User) => void;
  clearAuth: () => void;
}

// Simple storage for React Native compatibility
const storage = {
  getItem: (name: string) => {
    try {
      if (typeof localStorage !== 'undefined') {
        return localStorage.getItem(name);
      }
      return null;
    } catch {
      return null;
    }
  },
  setItem: (name: string, value: string) => {
    try {
      if (typeof localStorage !== 'undefined') {
        localStorage.setItem(name, value);
      }
    } catch {
      // Ignore storage errors
    }
  },
  removeItem: (name: string) => {
    try {
      if (typeof localStorage !== 'undefined') {
        localStorage.removeItem(name);
      }
    } catch {
      // Ignore storage errors
    }
  }
};

export const useAuthStore = create<AuthState>()((set) => ({
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
  setAuth: (auth: AuthResponse, user: User) => {
    set({
      user,
      accessToken: auth.accessToken,
      refreshToken: auth.refreshToken,
      isAuthenticated: true,
    });
    // Also store in storage for API client
    storage.setItem('accessToken', auth.accessToken);
    storage.setItem('refreshToken', auth.refreshToken);
    storage.setItem('user', JSON.stringify(user));
  },
  clearAuth: () => {
    set({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    });
    storage.removeItem('accessToken');
    storage.removeItem('refreshToken');
    storage.removeItem('user');
  },
}));