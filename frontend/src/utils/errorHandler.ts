import { AxiosError } from 'axios';
import { Alert } from 'react-native';
import { ErrorResponse } from '../types';

/**
 * Extracts error message from various error types
 */
export const getErrorMessage = (error: unknown): string => {
  if (error instanceof AxiosError) {
    // Handle Axios errors
    if (error.response?.data) {
      const data = error.response.data;

      // Check for ErrorResponse format
      if (typeof data === 'object' && 'message' in data) {
        return data.message;
      }

      // Check for validation errors
      if (typeof data === 'object' && 'errors' in data) {
        const errors = data.errors as Record<string, string[]>;
        const firstError = Object.values(errors)[0];
        if (firstError && firstError.length > 0) {
          return firstError[0];
        }
      }

      // Check for string message
      if (typeof data === 'string') {
        return data;
      }
    }

    // Network errors
    if (error.code === 'ECONNABORTED') {
      return 'Request timeout. Please try again.';
    }
    if (error.code === 'ERR_NETWORK') {
      return 'Network error. Please check your connection.';
    }

    // Fallback to error message
    return error.message || 'An unexpected error occurred';
  }

  // Handle regular Error objects
  if (error instanceof Error) {
    return error.message;
  }

  // Handle string errors
  if (typeof error === 'string') {
    return error;
  }

  // Default message
  return 'An unexpected error occurred';
};

/**
 * Shows an error alert to the user
 */
export const showErrorAlert = (
  title: string,
  error: unknown,
  onDismiss?: () => void
): void => {
  const message = getErrorMessage(error);

  Alert.alert(
    title,
    message,
    [
      {
        text: 'OK',
        onPress: onDismiss,
        style: 'default',
      },
    ],
    { cancelable: true }
  );
};

/**
 * Handles API errors with appropriate user feedback
 */
export const handleApiError = (error: unknown, context?: string): void => {
  const title = context || 'Error';

  if (error instanceof AxiosError) {
    const status = error.response?.status;

    switch (status) {
      case 401:
        // Unauthorized - handled by axios interceptor
        break;
      case 403:
        showErrorAlert(title, 'You do not have permission to perform this action');
        break;
      case 404:
        showErrorAlert(title, 'The requested resource was not found');
        break;
      case 422:
      case 400:
        showErrorAlert('Validation Error', error);
        break;
      case 500:
        showErrorAlert('Server Error', 'Something went wrong on our end. Please try again later.');
        break;
      case 503:
        showErrorAlert('Service Unavailable', 'The service is temporarily unavailable. Please try again later.');
        break;
      default:
        showErrorAlert(title, error);
    }
  } else {
    showErrorAlert(title, error);
  }
};

/**
 * Formats validation errors for display
 */
export const formatValidationErrors = (errors: Record<string, string[]>): string => {
  const errorMessages: string[] = [];

  Object.entries(errors).forEach(([field, messages]) => {
    messages.forEach((message) => {
      errorMessages.push(`${field}: ${message}`);
    });
  });

  return errorMessages.join('\n');
};

/**
 * Checks if an error is a network error
 */
export const isNetworkError = (error: unknown): boolean => {
  if (error instanceof AxiosError) {
    return !error.response && (
      error.code === 'ERR_NETWORK' ||
      error.code === 'ECONNABORTED' ||
      error.message === 'Network Error'
    );
  }
  return false;
};

/**
 * Checks if an error is an authentication error
 */
export const isAuthError = (error: unknown): boolean => {
  if (error instanceof AxiosError) {
    return error.response?.status === 401;
  }
  return false;
};

/**
 * Logs error for debugging (in development only)
 */
export const logError = (error: unknown, context?: string): void => {
  if (__DEV__) {
    console.error(`Error in ${context || 'Unknown context'}:`, error);

    if (error instanceof AxiosError) {
      console.error('Response data:', error.response?.data);
      console.error('Response status:', error.response?.status);
      console.error('Response headers:', error.response?.headers);
    }
  }
};