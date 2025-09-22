# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Tinder-like dating application with:
- **Backend**: ASP.NET Core (.NET 9) Web API with PostgreSQL/PostGIS
- **Frontend**: Expo React Native (TypeScript) mobile app
- **Real-time**: SignalR for live chat and notifications

## CRITICAL: iOS Login Issue Fix

### Problem
iOS app cannot login while web works. This is due to API URL configuration.

### Solution
The API URL must be configured correctly for iOS in `/frontend/src/config/api.ts`:

```typescript
const getApiBaseUrl = () => {
  if (Platform.OS === 'ios') {
    // For iOS Simulator on same machine
    return 'http://localhost:8080/api';
    // For physical iOS device, use Mac's IP:
    // return 'http://10.39.42.221:8080/api';
  } else if (Platform.OS === 'android') {
    return 'http://10.0.2.2:8080/api';
  } else {
    return 'http://localhost:8080/api';
  }
};
```

### Backend Must Listen on All Interfaces
```bash
ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Development dotnet run
```

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

The database includes test users (all with password `Password123!`):
- alice@example.com - Female, 28, San Francisco
- bob@example.com - Male, 32, San Francisco
- charlie@example.com - Male, 29, San Francisco
- diana@example.com - Female, 26, San Francisco
- eve@example.com - Female, 31, San Francisco
- frank@example.com - Male, 27, San Francisco
- grace@example.com - Female, 30, San Francisco
- henry@example.com - Male, 34, San Francisco

To apply test data:
```bash
docker exec -i tinder-clone-db-1 psql -U appuser -d appdb < backend/App/Migrations/AddCompleteTestData.sql
```

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

## Troubleshooting

### Common Issues and Fixes

1. **.NET Version Mismatch**
   - Error: Project targets .NET 8 but only .NET 9 installed
   - Fix: Update all .csproj files to `<TargetFramework>net9.0</TargetFramework>`

2. **DateTime UTC Error**
   - Error: "Cannot write DateTime with Kind=Unspecified to PostgreSQL"
   - Fix: Use `DateTime.SpecifyKind(date, DateTimeKind.Utc)` in AuthService.cs

3. **iOS Cannot Login**
   - Ensure backend runs with `ASPNETCORE_URLS=http://+:8080`
   - Update `/frontend/src/config/api.ts` with correct iOS URL
   - For simulator: `http://localhost:8080/api`
   - For device: `http://YOUR_MAC_IP:8080/api`

4. **Docker Connection Issues**
   - Restart Docker Desktop
   - Clean rebuild: `docker-compose down -v && docker-compose up --build`

5. **Missing Test Data**
   - Apply migrations: `docker exec -i tinder-clone-db-1 psql -U appuser -d appdb < backend/App/Migrations/AddCompleteTestData.sql`
- to memorize