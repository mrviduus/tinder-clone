export interface User {
  id: string;
  email: string;
  displayName: string;
  birthDate: string;
  gender: Gender;
}

export enum Gender {
  Unknown = 0,
  Male = 1,
  Female = 2,
  NonBinary = 3,
}

export interface Profile {
  userId: string;
  displayName: string;
  birthDate: string;
  gender: Gender;
  bio?: string;
  jobTitle?: string;
  company?: string;
  school?: string;
  location?: {
    latitude: number;
    longitude: number;
  };
  photos: Photo[];
  isCurrentUser?: boolean;
}

export interface Photo {
  photoId: string;
  userId: string;
  photoData: string; // base64 encoded
  uploadedAt: string;
  isMain: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  birthDate: string;
  gender: Gender;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  accessExpiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
}

export interface RegisterResponse {
  userId: string;
}

export interface UpdateProfileRequest {
  displayName?: string;
  bio?: string;
  jobTitle?: string;
  company?: string;
  school?: string;
}

export interface UpdateLocationRequest {
  latitude: number;
  longitude: number;
}

export interface SwipeRequest {
  targetUserId: string;
  direction: string; // "like" or "pass" - backend expects string
}

export enum SwipeDirection {
  Pass = 0,
  Like = 1,
}

export interface SwipeResult {
  isMatch: boolean;
  matchId?: string;
}

export interface Match {
  matchId: string;
  userId: string;
  displayName: string;
  photos: Photo[];
  matchedAt: string;
  lastMessageAt?: string;
  lastMessage?: string;
}

export interface Message {
  messageId: string;
  matchId: string;
  senderId: string;
  content: string;
  sentAt: string;
  readAt?: string;
}

export interface SendMessageRequest {
  matchId: string;
  text: string;
  photoId?: string;
}

export interface ErrorResponse {
  error: string;
  traceId: string;
}