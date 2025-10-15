# Docker Compose Guide - Tinder Clone

## 🐳 Overview

Docker Compose orchestrates the entire application stack with 5 services:
- **db**: PostgreSQL with PostGIS for location features
- **api**: .NET 9 backend API
- **frontend**: Expo React Native web app
- **pgadmin**: Database management UI (optional)
- **redis**: Cache for scaling (optional)

## 🚀 Quick Start

### Basic Usage (All Services)
```bash
# Start everything (database, backend, frontend)
docker-compose up

# Start in background
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

## 📦 Service-Specific Commands

### Database Only
```bash
# Start just the database
docker-compose up -d db

# Check database status
docker-compose ps db

# Access database shell
docker exec -it tinder-clone-db-1 psql -U appuser -d appdb
```

### Backend API Only
```bash
# Start database and API
docker-compose up -d db api

# View API logs
docker-compose logs -f api

# Rebuild API after code changes
docker-compose build api
docker-compose up -d api
```

### Frontend Only
```bash
# Start everything needed for frontend
docker-compose up -d db api frontend

# View frontend logs
docker-compose logs -f frontend

# Access frontend
# Web: http://localhost:19006
```

## 🔧 Service Profiles

### Migration Profile
```bash
# Run database migrations
docker-compose --profile migration up migration

# This runs EF Core migrations automatically
```

### Tools Profile (pgAdmin)
```bash
# Start with pgAdmin for database management
docker-compose --profile tools up -d

# Access pgAdmin at http://localhost:5050
# Login: admin@example.com / admin
```

### Scale Profile (Redis)
```bash
# Start with Redis for horizontal scaling
docker-compose --profile scale up -d

# Redis available at localhost:6379
```

## 🌐 Service URLs

| Service | URL | Description |
|---------|-----|-------------|
| Backend API | http://localhost:8080 | REST API endpoints |
| Swagger UI | http://localhost:8080/swagger | API documentation |
| Frontend Web | http://localhost:19006 | Expo web interface |
| pgAdmin | http://localhost:5050 | Database management |
| PostgreSQL | localhost:5432 | Direct DB connection |

## 📊 Environment Variables

Create `.env` file (optional):
```env
POSTGRES_DB=appdb
POSTGRES_USER=appuser
POSTGRES_PASSWORD=appsecret
JWT_KEY=your-super-secret-jwt-key-minimum-32-characters
```

## 🛠️ Common Workflows

### 1. Fresh Start (Clean Database)
```bash
# Stop everything and remove data
docker-compose down -v

# Start fresh
docker-compose up --build

# Apply migrations
docker-compose --profile migration up migration

# Apply test data
docker exec -i tinder-clone-db-1 psql -U appuser -d appdb < backend/App/Migrations/AddCompleteTestData.sql
```

### 2. Development Workflow
```bash
# Start services in background
docker-compose up -d

# Watch logs
docker-compose logs -f api

# Make code changes, then rebuild
docker-compose build api
docker-compose restart api
```

### 3. Database Management
```bash
# Start with pgAdmin
docker-compose --profile tools up -d

# Connect to pgAdmin
# Server: db
# Port: 5432
# Database: appdb
# Username: appuser
# Password: appsecret
```

### 4. Debugging Issues
```bash
# Check service status
docker-compose ps

# View specific service logs
docker-compose logs api
docker-compose logs db
docker-compose logs frontend

# Inspect running container
docker-compose exec api bash
docker-compose exec db psql -U appuser -d appdb

# Restart specific service
docker-compose restart api
```

## 🔄 Updates After Code Changes

### Backend Changes
```bash
# After modifying C# code
docker-compose build api
docker-compose up -d api
```

### Frontend Changes
```bash
# Frontend has hot reload, just save files
# If package.json changed:
docker-compose build frontend
docker-compose up -d frontend
```

### Database Schema Changes
```bash
# Create new migration (run locally)
cd backend/App
dotnet ef migrations add YourMigrationName

# Apply via Docker
docker-compose --profile migration up migration
```

## ⚠️ Troubleshooting

### Port Already in Use
```bash
# Check what's using the port
lsof -i :8080  # or 5432, 19006, etc.

# Change port in docker-compose.yml
ports:
  - "8081:8080"  # Use 8081 externally
```

### Database Connection Issues
```bash
# Ensure database is healthy
docker-compose ps db

# Check logs
docker-compose logs db

# Restart database
docker-compose restart db
```

### Build Failures
```bash
# Clean rebuild
docker-compose build --no-cache api

# Remove all containers and volumes
docker-compose down -v
docker system prune -a
```

## 📝 Notes

- **Database persists** in `pgdata` volume between restarts
- **Frontend changes** hot-reload automatically
- **API changes** require rebuild and restart
- **Migrations** should be run after database starts
- **.NET 9** is now configured (updated from .NET 8)

## 🚨 Current Status

✅ **Working**:
- Database (PostgreSQL 16 with PostGIS)
- Backend API (.NET 9)
- All Docker images updated to .NET 9

⚠️ **To Test**:
- Frontend container (Expo)
- Full stack integration

## 💡 Tips

1. **Use profiles** for optional services (pgAdmin, Redis)
2. **Check health** before starting dependent services
3. **Volume mounts** allow hot reload for frontend
4. **Named volumes** persist database between restarts
5. **Environment variables** can override defaults