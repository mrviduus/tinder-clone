# Quick Start Guide: Tinder Clone

## Prerequisites

- Docker Desktop installed and running
- Git installed
- 8GB+ RAM available
- Ports 8080, 19006, 5432 available

## Setup (Under 5 Minutes)

### 1. Clone and Configure

```bash
git clone https://github.com/your-repo/tinder-clone.git
cd tinder-clone
cp .env.example .env
```

### 2. Start Everything

```bash
docker-compose up
```

This single command will:
- ✅ Create PostgreSQL database with PostGIS
- ✅ Run all database migrations
- ✅ Seed test users and data
- ✅ Start backend API on http://localhost:8080
- ✅ Start frontend on http://localhost:19006
- ✅ Enable hot-reload for development

### 3. Verify Services

- **API Health**: http://localhost:8080/health
- **Swagger UI**: http://localhost:8080/swagger
- **Frontend Web**: http://localhost:19006
- **pgAdmin** (optional): http://localhost:5050
  - Email: admin@example.com
  - Password: admin

## Test Accounts

All test users have password: `Password123!`

| Email | Gender | Age | Seeking | Location |
|-------|--------|-----|---------|----------|
| alice@example.com | Female | 28 | Males | San Francisco |
| bob@example.com | Male | 32 | Females | San Francisco |
| charlie@example.com | Male | 29 | Females | San Francisco |
| diana@example.com | Female | 26 | Males | San Francisco |
| eve@example.com | Female | 31 | Males | San Francisco |
| frank@example.com | Male | 27 | Females | San Francisco |
| grace@example.com | Female | 30 | Males | San Francisco |
| henry@example.com | Male | 34 | Females | San Francisco |

## Development Workflow

### Backend Development

```bash
# Run backend locally (without Docker)
cd backend/App
dotnet run

# Run tests
cd backend/App.Tests
dotnet test

# Add new migration
cd backend/App
dotnet ef migrations add YourMigrationName

# Apply migrations
dotnet ef database update
```

### Frontend Development

```bash
# Run frontend locally
cd frontend
npm install
npm run web    # Web version
npm run ios    # iOS simulator
npm run android # Android emulator

# Run tests
npm test

# Type checking
npx tsc --noEmit
```

### Mobile Testing

#### iOS Simulator
```bash
# Frontend must use localhost
# API URL: http://localhost:8080/api
npm run ios
```

#### iOS Device
```bash
# Find your Mac's IP
ifconfig | grep inet

# Update frontend/src/config/api.ts with your IP
# API URL: http://YOUR_MAC_IP:8080/api

# Backend must listen on all interfaces
ASPNETCORE_URLS=http://+:8080 dotnet run
```

#### Android Emulator
```bash
# Frontend uses special IP
# API URL: http://10.0.2.2:8080/api
npm run android
```

## API Testing

### Using cURL

```bash
# Register new user
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123!",
    "displayName": "Test User",
    "birthDate": "1995-01-01",
    "gender": "male"
  }'

# Login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Password123!"
  }'

# Get feed (requires auth token)
curl -X GET http://localhost:8080/api/feed \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

### Using Swagger UI

1. Navigate to http://localhost:8080/swagger
2. Use "Authorize" button to add JWT token
3. Test endpoints interactively

## Real-time Features (SignalR)

### JavaScript Client Example

```javascript
import { HubConnectionBuilder } from '@microsoft/signalr';

// Create connection
const connection = new HubConnectionBuilder()
  .withUrl('http://localhost:8080/hubs/chat', {
    accessTokenFactory: () => getAccessToken()
  })
  .withAutomaticReconnect()
  .build();

// Register handlers
connection.on('ReceiveMessage', (message) => {
  console.log('New message:', message);
});

connection.on('UserTyping', (matchId, userId, isTyping) => {
  console.log(`User ${userId} is ${isTyping ? 'typing' : 'stopped typing'}`);
});

connection.on('NewMatch', (match) => {
  console.log('New match!', match);
});

// Start connection
await connection.start();

// Join match group
await connection.invoke('JoinMatch', matchId);

// Send message
await connection.invoke('SendMessage', matchId, 'Hello!');

// Typing indicators
await connection.invoke('StartTyping', matchId);
await connection.invoke('StopTyping', matchId);
```

## Common Tasks

### Reset Database

```bash
docker-compose down -v
docker-compose up
```

### View Logs

```bash
# All logs
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f db
```

### Update Dependencies

```bash
# Backend
cd backend/App
dotnet add package PackageName

# Frontend
cd frontend
npm install package-name
```

### Build for Production

```bash
# Backend
cd backend
docker build -t tinder-api .

# Frontend
cd frontend
npm run build
```

## Troubleshooting

### Port Already in Use

```bash
# Find process using port
lsof -i :8080

# Kill process
kill -9 PID
```

### Database Connection Issues

```bash
# Check if database is running
docker ps

# Check database logs
docker logs tinder-clone-db-1

# Connect manually
psql -h localhost -U appuser -d appdb
# Password: appsecret
```

### iOS Can't Connect to API

1. Ensure backend runs with: `ASPNETCORE_URLS=http://+:8080`
2. Update `frontend/src/config/api.ts` with correct URL
3. For simulator: use `http://localhost:8080/api`
4. For device: use `http://YOUR_MAC_IP:8080/api`

### Docker Issues

```bash
# Clean restart
docker-compose down -v
docker system prune -a
docker-compose up --build

# Reset Docker Desktop
# Open Docker Desktop → Preferences → Reset → Reset to factory defaults
```

## Next Steps

1. **Explore the API**: Use Swagger UI at http://localhost:8080/swagger
2. **Try the App**: Login as Alice or Bob and start swiping
3. **Check Real-time**: Open two browsers, login as different users, match and chat
4. **Customize**: Edit profile, upload photos, change preferences
5. **Develop**: Make changes, see hot-reload in action

## Resources

- [API Documentation](http://localhost:8080/swagger)
- [Project Constitution](./.specify/memory/constitution.md)
- [Data Model](./specs/001-tinder-clone-core/data-model.md)
- [Architecture Overview](./CLAUDE.md)

## Support

- GitHub Issues: https://github.com/your-repo/tinder-clone/issues
- Documentation: See `/docs` folder
- Test Coverage: Run `dotnet test --collect:"XPlat Code Coverage"`