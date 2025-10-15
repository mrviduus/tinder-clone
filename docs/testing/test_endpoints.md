# API Endpoints Test Plan

## Authentication Endpoints
- ✅ POST /api/auth/register
- ✅ POST /api/auth/login
- ✅ POST /api/auth/refresh
- ✅ POST /api/auth/logout

## Profile Endpoints
- ✅ GET /api/me
- ✅ PUT /api/me
- ✅ PUT /api/me/location
- ✅ POST /api/me/photos
- ✅ DELETE /api/me/photos/{photoId}
- ✅ GET /api/users/{id}
- ✅ GET /api/users/{userId}/photos/{photoId}

## Feed & Swipes
- ✅ GET /api/feed
- ✅ POST /api/swipes

## Matches & Messages
- ✅ GET /api/matches
- ✅ GET /api/matches/{matchId}
- ✅ GET /api/matches/{matchId}/messages
- ✅ POST /api/matches/{matchId}/messages

## SignalR Hub
- ✅ /hubs/chat

## Database Schema
- ✅ Users table with ASP.NET Identity
- ✅ Profiles with PostGIS location support
- ✅ Photos with BLOB storage
- ✅ Swipes with unique constraints
- ✅ Matches with proper relationships
- ✅ Messages with read receipts
- ✅ Blocks for user blocking
- ✅ Refresh tokens for JWT

## Frontend Structure
- ✅ Expo React Native project
- ✅ TypeScript configuration
- ✅ Navigation (Auth Stack + Main Tabs)
- ✅ Zustand state management
- ✅ API layer with axios
- ✅ SignalR integration
- ✅ Authentication screens
- ✅ Feed with swipe gestures
- ✅ Matches list
- ✅ Chat interface
- ✅ Profile management

## Docker Configuration
- ✅ PostgreSQL with PostGIS
- ✅ ASP.NET Core backend
- ✅ Expo frontend
- ✅ Environment variables
- ✅ Health checks
- ✅ Proper networking