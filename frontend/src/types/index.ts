// Common types
export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: Record<string, string[]>;
  statusCode: number;
  timestamp: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ErrorResponse {
  message: string;
  details?: Record<string, string[]>;
  errorCode?: string;
  timestamp: string;
  traceId?: string;
}

// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  birthDate: string;
  gender: 'male' | 'female' | 'other';
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// User types
export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  age: number;
  gender: string;
}

// Profile types
export interface ProfileDto {
  id: string;
  userId: string;
  displayName: string;
  bio?: string;
  age: number;
  gender: string;
  city?: string;
  photos: PhotoDto[];
  distance?: number;
}

export interface UpdateProfileRequest {
  displayName?: string;
  bio?: string;
  seekingGender?: string;
  maxDistance?: number;
  ageRangeMin?: number;
  ageRangeMax?: number;
}

export interface UpdateLocationRequest {
  latitude: number;
  longitude: number;
}

// Photo types
export interface PhotoDto {
  id: string;
  url: string;
  isPrimary: boolean;
  orderIndex: number;
}

// Feed & Swipe types
export interface FeedProfile {
  userId: string;
  displayName: string;
  age: number;
  bio?: string;
  photos: PhotoDto[];
  distance: number;
  city?: string;
}

export interface SwipeRequest {
  targetUserId: string;
  direction: 'Like' | 'Pass';
  isSuperLike?: boolean;
}

export interface SwipeResponse {
  isMatch: boolean;
  matchId?: string;
}

// Match types
export interface MatchDto {
  id: string;
  matchedUser: ProfileDto;
  matchedAt: string;
  lastMessage?: MessageDto;
  unreadCount: number;
}

// Message types
export interface MessageDto {
  id: string;
  matchId: string;
  senderId: string;
  recipientId: string;
  content: string;
  sentAt: string;
  readAt?: string;
}

export interface SendMessageRequest {
  matchId: string;
  content: string;
}

// Navigation types
export type RootStackParamList = {
  Auth: undefined;
  Main: undefined;
  Login: undefined;
  Register: undefined;
  Profile: undefined;
  EditProfile: undefined;
  Feed: undefined;
  Matches: undefined;
  Chat: { matchId: string };
  ChatList: undefined;
};

export type MainTabParamList = {
  Feed: undefined;
  Matches: undefined;
  Messages: undefined;
  Profile: undefined;
};

// Store types
export interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  setAuth: (response: AuthResponse) => void;
  clearAuth: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export interface ProfileState {
  profile: ProfileDto | null;
  isLoading: boolean;
  error: string | null;
  setProfile: (profile: ProfileDto) => void;
  updateProfile: (updates: Partial<ProfileDto>) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export interface FeedState {
  profiles: FeedProfile[];
  currentIndex: number;
  isLoading: boolean;
  error: string | null;
  setProfiles: (profiles: FeedProfile[]) => void;
  nextProfile: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export interface MatchState {
  matches: MatchDto[];
  isLoading: boolean;
  error: string | null;
  setMatches: (matches: MatchDto[]) => void;
  addMatch: (match: MatchDto) => void;
  removeMatch: (matchId: string) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export interface ChatState {
  messages: Record<string, MessageDto[]>;
  typingUsers: Record<string, boolean>;
  isLoading: boolean;
  error: string | null;
  setMessages: (matchId: string, messages: MessageDto[]) => void;
  addMessage: (matchId: string, message: MessageDto) => void;
  setTyping: (matchId: string, userId: string, isTyping: boolean) => void;
  markAsRead: (messageIds: string[]) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}