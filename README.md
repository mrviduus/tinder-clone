# Tinder Clone - Full Stack Dating Application

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![React Native](https://img.shields.io/badge/React_Native-0.74-61DAFB?style=for-the-badge&logo=react&logoColor=black)
![Expo](https://img.shields.io/badge/Expo-SDK_51-000020?style=for-the-badge&logo=expo&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-007ACC?style=for-the-badge&logo=typescript&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

A production-ready Tinder clone featuring real-time messaging, geolocation-based matching, and a modern mobile-first design. Built with **ASP.NET Core**, **React Native (Expo)**, **PostgreSQL/PostGIS**, and **SignalR** for real-time communication.

## 🌟 Key Features

### Core Functionality
- 👤 **User Authentication** - JWT-based authentication with refresh token rotation
- 📍 **Location-Based Matching** - Find nearby users using PostGIS spatial queries
- 💖 **Swipe Mechanism** - Intuitive like/pass gestures with smooth animations
- 🎯 **Smart Matching Algorithm** - Instant matches on mutual likes
- 💬 **Real-Time Chat** - SignalR-powered messaging with typing indicators
- 📸 **Photo Management** - Multiple photo uploads with primary photo selection
- 🔔 **Live Notifications** - Real-time match and message notifications

### Technical Highlights
- **Clean Architecture** - Domain-driven design with separation of concerns
- **Type Safety** - Full TypeScript support on frontend, C# 12 on backend
- **Geospatial Queries** - Efficient location-based searches with PostGIS
- **Real-Time Updates** - WebSocket connections for instant updates
- **Token Rotation** - Secure JWT refresh token mechanism
- **Docker Ready** - Complete containerization for easy deployment
- **Cross-Platform** - iOS, Android, and Web support via Expo

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- Node.js 20+ (for local development)
- .NET 9 SDK (for local development)
- iOS Simulator / Android Emulator (optional)

### 🐳 Docker Deployment (Recommended)

```bash
# Clone the repository
git clone https://github.com/vasylvdovychenko/tinder-clone.git
cd tinder-clone

# Start all services
docker-compose up --build

# Apply database migrations and seed data
docker-compose --profile migration up
```

Access the services:
- 🌐 **API**: http://localhost:8080
- 📱 **Web App**: http://localhost:19006
- 📖 **Swagger**: http://localhost:8080/swagger
- 🗄️ **pgAdmin**: http://localhost:5050

### 💻 Local Development

#### Backend Setup
```bash
cd backend/App
dotnet restore
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

#### Frontend Setup
```bash
cd frontend
npm install
npm run web    # Web version
npm run ios    # iOS Simulator
npm run android # Android Emulator
```

## 🏗️ Architecture

### System Design
```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│  React Native   │────▶│  ASP.NET Core   │────▶│  PostgreSQL     │
│   Expo (TS)     │◀────│    Web API      │◀────│   + PostGIS     │
│                 │     │                 │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │
         │                       │
         └──────SignalR──────────┘
           (Real-time Chat)
```

### Backend Structure
- **Clean Architecture Pattern**
  - `Controllers/` - HTTP endpoints
  - `Services/` - Business logic layer
  - `Domain/` - Entity models
  - `DTOs/` - Data transfer objects
  - `Hubs/` - SignalR real-time hubs
  - `Middleware/` - Custom middleware

### Frontend Structure
- **Component-Based Architecture**
  - `screens/` - Application screens
  - `services/` - API integration
  - `store/` - Global state (Zustand)
  - `components/` - Reusable components
  - `types/` - TypeScript definitions

### Database Schema
- **PostGIS-enabled PostgreSQL**
  - Spatial indexing for location queries
  - Optimized for geospatial operations
  - Efficient match and message retrieval

## 🧪 Testing

### Run Tests
```bash
# Backend tests
cd backend/App.Tests
dotnet test

# Or use the test script
./scripts/testing/test.sh
```

### Test Users
The application includes pre-configured test users (all with password `Password123!`):
- alice@example.com - Female, 28, San Francisco
- bob@example.com - Male, 32, San Francisco
- charlie@example.com - Male, 29, San Francisco
- diana@example.com - Female, 26, San Francisco
- eve@example.com - Female, 31, San Francisco

## 📱 Supported Platforms

| Platform | Status | Notes |
|----------|--------|-------|
| iOS | ✅ Fully Supported | Requires Xcode & iOS Simulator |
| Android | ✅ Fully Supported | Requires Android Studio |
| Web | ✅ Fully Supported | Progressive Web App |

## 🔧 Configuration

### Environment Variables
See `scripts/docker/.env.docker` for Docker configuration.

### API Configuration
The API URL configuration handles platform-specific networking:
- iOS Simulator: `http://localhost:8080/api`
- Android Emulator: `http://10.0.2.2:8080/api`
- Physical Device: `http://YOUR_IP:8080/api`

## 📚 Documentation

- [📋 Requirements & Specifications](./docs/requirements.md)
- [🏛️ Architecture Documentation](./docs/architecture/)
- [🚀 Setup Guides](./docs/setup/)
- [🧪 Testing Documentation](./docs/testing/)
- [💻 Development History](./docs/development/)
- [💾 Database Architecture](./docs/DATABASE_ARCHITECTURE.md)
- [💬 Real-Time Chat Documentation](./docs/REAL_TIME_CHAT_DOCUMENTATION.md)

## 🤝 Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

### Development Workflow
1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📊 Project Status

This project is actively maintained and welcomes contributions. It serves as a comprehensive example of modern full-stack development with:
- ✅ Production-ready architecture
- ✅ Industry best practices
- ✅ Comprehensive documentation
- ✅ Clean, maintainable code
- ✅ Full type safety

## 🐛 Known Issues

- Photo upload on web requires fallback to file input
- iOS physical device requires IP configuration
- SignalR reconnection needs manual handling in some edge cases

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Expo team for cross-platform mobile development
- PostGIS for geospatial capabilities
- SignalR for real-time functionality

## 📞 Contact

**Vasyl Vdovychenko** - [GitHub Profile](https://github.com/vasylvdovychenko)

Project Link: [https://github.com/vasylvdovychenko/tinder-clone](https://github.com/vasylvdovychenko/tinder-clone)

---

<p align="center">Made with ❤️ by a passionate developer</p>
<p align="center">⭐ If you find this project useful, please consider giving it a star!</p>