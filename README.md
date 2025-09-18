# Tinder MVP

A minimal viable Tinder-like dating application built with ASP.NET Core backend, PostgreSQL + PostGIS database, and Expo React Native frontend.

## Features

- ✅ User registration and authentication (JWT + refresh tokens)
- ✅ User profiles with photos and preferences
- ✅ Location-based candidate discovery
- ✅ Swipe gestures (like/pass)
- ✅ Real-time matching system
- ✅ Live chat with SignalR
- ✅ Message read receipts and typing indicators
- ✅ Photo upload and management
- ✅ Distance-based filtering

## Tech Stack

### Backend
- ASP.NET Core (.NET 9) Web API
- Entity Framework Core with PostgreSQL
- PostGIS for geospatial queries
- ASP.NET Identity for authentication
- JWT tokens with refresh token rotation
- SignalR for real-time chat
- Serilog for logging
- Health checks

### Frontend
- Expo React Native (TypeScript)
- React Navigation for routing
- Zustand for state management
- Axios for API calls
- SignalR client for real-time features
- React Native Reanimated for swipe gestures

### Database
- PostgreSQL 16 with PostGIS extension
- Optimized for location-based queries
- Proper indexing for performance

## Quick Start

1. **Clone and setup environment:**
   ```bash
   git clone <repo>
   cd tinder-mvp
   cp .env.example .env
   ```

2. **Start with Docker Compose:**
   ```bash
   docker compose up --build
   ```

3. **Access the application:**
   - Backend API: http://localhost:8080
   - Swagger UI: http://localhost:8080/swagger
   - Frontend Web: http://localhost:19006
   - pgAdmin (optional): http://localhost:5050

## Development Setup

### Prerequisites
- Docker & Docker Compose
- .NET 9 SDK (for local development)
- Node.js 20+ (for local frontend development)

### Backend Development
```bash
cd backend/App
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend Development
```bash
cd frontend
npm install
npm run start:web
```

### Database Access
- **Connection String:** Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=appsecret
- **pgAdmin:** http://localhost:5050 (admin@example.com / admin)

## Project Structure

```
.
├── docker-compose.yml          # Container orchestration
├── .env.example               # Environment variables template
├── requirements.md            # Detailed requirements specification
├── backend/
│   ├── Dockerfile
│   └── App/                   # ASP.NET Core Web API
│       ├── Controllers/       # API endpoints
│       ├── Services/          # Business logic
│       ├── Data/             # EF Core DbContext
│       ├── Domain/           # Entity models
│       ├── DTOs/             # Request/response models
│       ├── Hubs/             # SignalR hubs
│       └── Migrations/       # Database migrations
└── frontend/
    ├── Dockerfile
    └── src/
        ├── screens/          # React Native screens
        ├── navigation/       # Navigation setup
        ├── api/             # API layer
        ├── store/           # Zustand stores
        └── types/           # TypeScript types
```

## API Documentation

When running in development mode, visit http://localhost:8080/swagger for interactive API documentation.

### Key Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/feed` - Get potential matches
- `POST /api/swipes` - Swipe on users
- `GET /api/matches` - Get user's matches
- `SignalR /hubs/chat` - Real-time chat

## Database Schema

- **users** - ASP.NET Identity users
- **profiles** - User profiles with location (PostGIS Point)
- **photos** - Photo storage with BLOB data
- **swipes** - User swipe history
- **matches** - Mutual likes
- **messages** - Chat messages with read receipts
- **blocks** - User blocking functionality
- **refresh_tokens** - JWT refresh token storage

## Configuration

### Environment Variables
See `.env.example` for all available configuration options.

Key settings:
- `ConnectionStrings__Default` - PostgreSQL connection
- `Jwt__Key` - JWT signing key (change in production!)
- `Photos__MaxBytes` - Maximum photo size (5MB default)
- `Cors__AllowedOrigins__0` - Frontend URL for CORS

### Photo Storage
Currently stores photos as BLOB data in PostgreSQL. For production, consider moving to object storage (S3, Azure Blob) with CDN.

## Testing

### Test Credentials
The application comes with pre-seeded test users for easy testing:

**Alice (Female, 25)**
- Email: `alice@example.com`
- Password: `Password123!`
- Profile: Has multiple photos, seeks males

**Bob (Male, 28)**
- Email: `bob@example.com`
- Password: `Password123!`
- Profile: Has multiple photos, seeks females

**Additional Test Users:**
- `charlie@example.com` / `Password123!` (Male, 30)
- `diana@example.com` / `Password123!` (Female, 26)
- `eve@example.com` / `Password123!` (Female, 24)

### Manual Testing
1. **Quick Test:** Login as Alice or Bob using the credentials above
2. **Photo Testing:** Both Alice and Bob have multiple photos already uploaded
3. **Matching:** Alice and Bob are compatible and will appear in each other's feeds
4. **Chat Testing:** Alice and Bob already have an existing match with chat history
5. **Full Flow:** Register new users to test the complete registration flow

### Health Check
Visit http://localhost:8080/healthz to verify backend health.

## Production Considerations

### Security
- Change JWT secret key
- Use HTTPS in production
- Implement rate limiting (.NET 9+ required)
- Add photo content validation
- Set up proper CORS policies

### Performance
- Add Redis for SignalR backplane (horizontal scaling)
- Move photos to object storage + CDN
- Implement database connection pooling
- Add caching for frequent queries

### Monitoring
- Set up application insights
- Add structured logging
- Monitor database performance
- Set up health check monitoring

## Known Limitations (MVP)

- No push notifications
- Basic photo validation
- Simple matching algorithm
- No video calls
- No premium features
- No social login integration
- No email verification
- Basic error handling on frontend

## License

This is a demo/educational project. Use at your own risk.