# Project Overview - Tinder Clone

## Repository Structure

```
tinder-clone/
├── backend/                    # ASP.NET Core Backend
│   ├── App/                   # Main API Application
│   │   ├── Controllers/       # HTTP endpoints
│   │   ├── Services/          # Business logic
│   │   ├── Domain/            # Entity models
│   │   ├── DTOs/              # Data transfer objects
│   │   ├── Hubs/              # SignalR hubs
│   │   ├── Middleware/        # Custom middleware
│   │   ├── Migrations/        # EF Core migrations
│   │   └── Data/              # DbContext and configurations
│   ├── App.Tests/             # Unit and integration tests
│   └── Dockerfile             # Backend container configuration
│
├── frontend/                   # React Native/Expo Frontend
│   ├── src/                   # Source code
│   │   ├── screens/           # App screens
│   │   ├── services/          # API services
│   │   ├── store/             # State management (Zustand)
│   │   ├── components/        # Reusable components
│   │   ├── config/            # Configuration files
│   │   └── types/             # TypeScript definitions
│   ├── assets/                # Images, fonts, etc.
│   └── Dockerfile             # Frontend container configuration
│
├── docs/                       # Documentation
│   ├── architecture/          # System architecture docs
│   ├── setup/                 # Setup and installation guides
│   ├── testing/               # Testing documentation
│   ├── development/           # Development history and fixes
│   └── api/                   # API documentation
│
├── scripts/                    # Utility Scripts
│   ├── docker/                # Docker-related scripts
│   ├── database/              # Database migration scripts
│   └── testing/               # Test execution scripts
│
├── .github/                    # GitHub Configuration
│   ├── workflows/             # CI/CD workflows
│   ├── ISSUE_TEMPLATE/        # Issue templates
│   └── pull_request_template.md
│
├── docker-compose.yml          # Container orchestration
├── README.md                   # Main documentation
├── CONTRIBUTING.md             # Contribution guidelines
├── LICENSE                     # MIT License
└── PROJECT_OVERVIEW.md         # This file
```

## Technology Stack

### Backend (.NET 9)
- **Framework**: ASP.NET Core Web API
- **Database**: PostgreSQL 16 with PostGIS
- **ORM**: Entity Framework Core
- **Authentication**: JWT with refresh tokens
- **Real-time**: SignalR
- **Documentation**: Swagger/OpenAPI

### Frontend (React Native)
- **Framework**: Expo SDK 51
- **Language**: TypeScript
- **State Management**: Zustand
- **Navigation**: React Navigation
- **API Client**: Axios
- **Real-time Client**: @microsoft/signalr

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **CI/CD**: GitHub Actions
- **Development Environment**: Cross-platform (Windows, macOS, Linux)

## Key Features

1. **Authentication System**
   - JWT-based authentication
   - Refresh token rotation
   - Secure password hashing

2. **User Profiles**
   - Photo uploads (multiple photos)
   - Bio and preferences
   - Location-based discovery

3. **Matching System**
   - Swipe gestures (like/pass)
   - Instant match on mutual likes
   - Unmatch functionality

4. **Real-time Chat**
   - SignalR WebSocket connections
   - Message delivery/read receipts
   - Typing indicators
   - Chat history persistence

5. **Geolocation Features**
   - PostGIS spatial queries
   - Distance-based filtering
   - Efficient nearby user discovery

## Development Workflow

### Quick Start
```bash
# Clone repository
git clone https://github.com/vasylvdovychenko/tinder-clone.git
cd tinder-clone

# Start with Docker
docker-compose up --build

# Apply migrations
docker-compose --profile migration up
```

### Local Development

#### Backend
```bash
cd backend/App
dotnet restore
dotnet build
dotnet run
```

#### Frontend
```bash
cd frontend
npm install
npm run web   # For web
npm run ios   # For iOS
npm run android # For Android
```

### Testing
```bash
# Backend tests
./scripts/testing/test.sh

# Or manually
cd backend/App.Tests
dotnet test
```

## Database Management

### Migrations
```bash
# Create new migration
./scripts/database/create_migration.sh MigrationName

# Apply migrations
./scripts/database/apply_migration.sh
```

### Test Data
The application includes comprehensive test data with 8 pre-configured users, all located in San Francisco with complete profiles and photos.

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user

### User Profile
- `GET /api/profile` - Get current user profile
- `PUT /api/profile` - Update profile
- `POST /api/profile/photos` - Upload photo
- `DELETE /api/profile/photos/{id}` - Delete photo

### Feed & Matching
- `GET /api/feed` - Get potential matches
- `POST /api/swipes` - Submit swipe decision
- `GET /api/matches` - Get user's matches
- `DELETE /api/matches/{matchId}` - Unmatch

### Messaging
- `GET /api/messages/{matchId}` - Get chat messages
- Real-time via SignalR hub at `/hubs/chat`

## Deployment Considerations

### Production Checklist
- [ ] Change JWT secret key
- [ ] Configure HTTPS/SSL
- [ ] Set up proper CORS policies
- [ ] Implement rate limiting
- [ ] Configure production database
- [ ] Set up monitoring/logging
- [ ] Implement CDN for photos
- [ ] Configure email service
- [ ] Set up backup strategy

### Scaling Considerations
- Use Redis for SignalR backplane
- Implement database read replicas
- Move to cloud storage for photos
- Implement caching strategy
- Consider microservices architecture

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or suggestions, please open an issue on GitHub or contact the maintainer.