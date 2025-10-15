import { apiClient } from '../config/api';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RefreshTokenRequest,
  UserDto,
} from '../types';
import { useAuthStore } from '../store/authStore';

export class AuthService {
  static async register(data: RegisterRequest): Promise<AuthResponse> {
    console.log('Registration attempt with:', data.email);
    try {
      const response = await apiClient.post<AuthResponse>('/auth/register', data);

      // Immediately login after registration
      const loginResponse = await this.login({
        email: data.email,
        password: data.password,
      });

      return loginResponse;
    } catch (error: any) {
      console.error('Registration failed:', error.message);
      console.error('Error details:', error.response?.data);
      throw this.handleError(error);
    }
  }

  static async login(data: LoginRequest): Promise<AuthResponse> {
    console.log('Login attempt with:', data.email);
    console.log('API URL:', apiClient.defaults.baseURL);
    try {
      const response = await apiClient.post<AuthResponse>('/auth/login', data);
      console.log('Login successful');

      // Store auth data
      await useAuthStore.getState().setAuth(response.data);

      return response.data;
    } catch (error: any) {
      console.error('Login failed:', error.message);
      console.error('Error details:', error.response?.data);
      throw this.handleError(error);
    }
  }

  static async refreshToken(refreshToken: string): Promise<AuthResponse> {
    try {
      const request: RefreshTokenRequest = { refreshToken };
      const response = await apiClient.post<AuthResponse>('/auth/refresh', request);

      // Update auth data in store
      await useAuthStore.getState().setAuth(response.data);

      return response.data;
    } catch (error: any) {
      console.error('Token refresh failed:', error.message);
      throw this.handleError(error);
    }
  }

  static async logout(): Promise<void> {
    try {
      const refreshToken = useAuthStore.getState().refreshToken;
      if (refreshToken) {
        await apiClient.post('/auth/logout', { refreshToken });
      }
    } catch (error) {
      console.error('Logout error (continuing anyway):', error);
    } finally {
      // Clear local auth state regardless
      await useAuthStore.getState().clearAuth();
    }
  }

  static async getCurrentUser(): Promise<UserDto> {
    try {
      const response = await apiClient.get<any>('/profile/me');
      const profile = response.data;

      // Map backend Profile to frontend User
      return {
        id: profile.userId,
        email: profile.email || '',
        displayName: profile.displayName,
        age: profile.age,
        gender: profile.gender,
      };
    } catch (error: any) {
      console.error('Get user failed:', error.message);
      throw this.handleError(error);
    }
  }

  private static handleError(error: any): Error {
    if (error.response?.data?.message) {
      return new Error(error.response.data.message);
    } else if (error.response?.data?.error) {
      return new Error(error.response.data.error);
    } else if (error.response?.data?.errors) {
      // Format validation errors
      const errors = error.response.data.errors;
      const messages = Object.keys(errors).map(key => {
        return `${key}: ${errors[key].join(', ')}`;
      }).join('; ');
      return new Error(messages);
    } else if (error.message) {
      return new Error(error.message);
    } else {
      return new Error('An unexpected error occurred');
    }
  }
}