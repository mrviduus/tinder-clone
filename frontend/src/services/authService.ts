import apiClient from '../config/api';
import {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  RegisterResponse,
  User,
} from '../types/api';

export class AuthService {
  static async register(data: RegisterRequest): Promise<RegisterResponse> {
    const response = await apiClient.post<RegisterResponse>('/auth/register', data);
    return response.data;
  }

  static async login(data: LoginRequest): Promise<AuthResponse> {
    console.log('Login attempt with:', data.email);
    console.log('API URL:', apiClient.defaults.baseURL);
    try {
      const response = await apiClient.post<AuthResponse>('/auth/login', data);
      console.log('Login successful');
      return response.data;
    } catch (error: any) {
      console.error('Login failed:', error.message);
      console.error('Error details:', error.response?.data);
      throw error;
    }
  }

  static async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/refresh', {
      refreshToken,
    });
    return response.data;
  }

  static async logout(refreshToken: string): Promise<void> {
    await apiClient.post('/auth/logout', { refreshToken });
  }

  static async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<any>('/Me');
    const profile = response.data;

    // Map backend Profile to frontend User
    return {
      id: profile.userId,
      email: '', // Not provided by profile endpoint
      displayName: profile.displayName,
      birthDate: profile.birthDate,
      gender: profile.gender,
    };
  }
}