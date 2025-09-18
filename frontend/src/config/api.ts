import axios from 'axios';

const API_BASE_URL = 'http://localhost:8080/api';

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

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = storage.getItem('accessToken');
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
        const refreshToken = storage.getItem('refreshToken');
        if (refreshToken) {
          const response = await axios.post(`${API_BASE_URL}/auth/refresh`, {
            refreshToken,
          });

          const { accessToken, refreshToken: newRefreshToken } = response.data;
          storage.setItem('accessToken', accessToken);
          storage.setItem('refreshToken', newRefreshToken);

          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        storage.removeItem('accessToken');
        storage.removeItem('refreshToken');
        // Redirect to login or emit logout event
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;