# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Tinder-like dating application with:
- **Backend**: ASP.NET Core (.NET 8) Web API with PostgreSQL/PostGIS
- **Frontend**: Expo React Native (TypeScript) mobile app
- **Real-time**: SignalR for live chat and notifications

## Common Development Commands

### Backend (.NET)
```bash
# Build and run backend
cd backend/App
dotnet restore
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Database operations
dotnet ef database update         # Apply migrations
dotnet ef migrations add <Name>    # Create new migration

# Run tests
cd backend/App.Tests
dotnet test
# Or use the test script
./backend/test.sh
```

### Frontend (React Native/Expo)
```bash
cd frontend
npm install
npm run web    # Run web version
npm run ios    # Run iOS simulator
npm run android # Run Android emulator
```

### Docker Development
```bash
# Start all services
docker-compose up --build

# Start with database migration
docker-compose --profile migration up --build

# Access services
# Backend API: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
# Frontend Web: http://localhost:19006
# pgAdmin: http://localhost:5050 (admin@example.com/admin)
```

## Architecture Overview

### Backend Structure
The backend follows a clean architecture pattern:

- **Controllers**: API endpoints handling HTTP requests
- **Services**: Business logic layer (AuthService, SwipeService, MatchService, MessageService, etc.)
- **Domain**: Entity models with proper relationships
- **Data/AppDbContext**: EF Core database context with PostGIS for geospatial queries
- **Hubs/ChatHub**: SignalR hub for real-time chat functionality
- **DTOs**: Request/Response models for API communication
- **Middleware**: JWT refresh token handling

Key architectural decisions:
- JWT authentication with refresh token rotation
- PostGIS for efficient location-based queries (finding nearby users)
- SignalR for real-time features (chat, typing indicators, read receipts)
- Photos stored as BLOBs in database (consider cloud storage for production)

### Frontend Structure
The frontend uses Expo with TypeScript:

- **screens**: React Native screens (Login, Register, Home, Matches, Messages, Profile)
- **services**: API service layer with Axios
- **store**: Zustand for global state management
- **config/api.ts**: API configuration and token management
- **types**: TypeScript interfaces

Key patterns:
- Token refresh handled automatically in API interceptors
- SignalR connection managed in signalrService
- Swipe gestures using react-native-deck-swiper
- Navigation with React Navigation

### Database Schema
PostgreSQL with PostGIS extension:
- **users**: ASP.NET Identity users
- **profiles**: User profiles with PostGIS Point for location
- **photos**: Photo storage (BLOB data)
- **swipes**: Swipe history (Like/Pass)
- **matches**: Mutual likes creating matches
- **messages**: Chat messages with read status
- **refresh_tokens**: JWT refresh token storage

## Test Users

The database includes test users (created via migrations):
- alice@example.com / Password123!
- bob@example.com / Password123!
- charlie@example.com / Password123!
- diana@example.com / Password123!
- eve@example.com / Password123!

## Key Implementation Details

### Matching System
- Users swipe on profiles from the feed
- Mutual likes create a match immediately
- Matches enable chat functionality
- Unmatch removes all messages and the match

### Real-time Features
- SignalR groups based on match IDs
- Typing indicators and read receipts
- Automatic reconnection handling

### Location Services
- PostGIS spatial queries for nearby users
- Distance calculation using geography type
- Efficient indexing for performance

### Authentication Flow
1. Login returns access token (15 min) and refresh token (14 days)
2. Refresh tokens are rotated on use
3. Frontend automatically refreshes expired tokens
4. SignalR uses JWT for authentication