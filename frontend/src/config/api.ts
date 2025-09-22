import axios from 'axios';
import { Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

// Dynamic API URL based on platform
const getApiBaseUrl = () => {
  if (Platform.OS === 'ios') {
    // For iOS Simulator, localhost works correctly
    // For physical iOS device, replace with your Mac's IP (e.g., 'http://192.168.1.5:8080/api')
    return 'http://localhost:8080/api';
  } else if (Platform.OS === 'android') {
    // Android emulator uses special IP
    return 'http://10.0.2.2:8080/api';
  } else {
    // Web
    return 'http://localhost:8080/api';
  }
};

const API_BASE_URL = getApiBaseUrl();

// Cross-platform storage for React Native and Web
const storage = {
  getItem: async (name: string) => {
    try {
      if (Platform.OS === 'web' && typeof localStorage !== 'undefined') {
        return localStorage.getItem(name);
      } else {
        return await AsyncStorage.getItem(name);
      }
    } catch {
      return null;
    }
  },
  setItem: async (name: string, value: string) => {
    try {
      if (Platform.OS === 'web' && typeof localStorage !== 'undefined') {
        localStorage.setItem(name, value);
      } else {
        await AsyncStorage.setItem(name, value);
      }
    } catch (error) {
      console.error('Storage setItem error:', error);
    }
  },
  removeItem: async (name: string) => {
    try {
      if (Platform.OS === 'web' && typeof localStorage !== 'undefined') {
        localStorage.removeItem(name);
      } else {
        await AsyncStorage.removeItem(name);
      }
    } catch (error) {
      console.error('Storage removeItem error:', error);
    }
  }
};

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  async (config) => {
    const token = await storage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = await storage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
          });

          const { accessToken, refreshToken: newRefreshToken } = response.data;
          await storage.setItem('accessToken', accessToken);
          await storage.setItem('refreshToken', newRefreshToken);

          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        await storage.removeItem('accessToken');
        await storage.removeItem('refreshToken');
        // Redirect to login or emit logout event
      }
    }

    return Promise.reject(error);
  }
);

export { storage };
export default apiClient;