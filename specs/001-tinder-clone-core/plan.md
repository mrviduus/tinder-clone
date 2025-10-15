# Implementation Plan: Tinder Clone Core

**Branch**: `001-tinder-clone-core` | **Date**: 2024-01-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-tinder-clone-core/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Building a complete dating application with user profiles, location-based matching, real-time chat, and swipe-based interactions. The system uses ASP.NET Core backend with PostgreSQL/PostGIS for spatial queries, React Native/Expo frontend for cross-platform mobile experience, and SignalR for real-time features.

## Technical Context

**Language/Version**: Backend: C# / .NET 9, Frontend: TypeScript / React Native 0.74
**Primary Dependencies**: ASP.NET Core, Entity Framework Core, SignalR, Expo SDK 51, React Navigation, Zustand
**Storage**: PostgreSQL 16 with PostGIS 3.4 extension for spatial data
**Testing**: Backend: xUnit, Frontend: Jest + React Native Testing Library
**Target Platform**: iOS, Android, Web (via Expo)
**Project Type**: web/mobile - Monorepo with backend API and cross-platform frontend
**Performance Goals**: Feed load < 500ms, Swipe < 200ms, Match calculation < 100ms, Message delivery < 50ms
**Constraints**: Photo size 5MB max, JWT tokens 15min expiry, Support 1000+ concurrent users
**Scale/Scope**: MVP with 8 core features, ~50 API endpoints, 10+ screens

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle Compliance

✅ **I. Clean Architecture**: Proper layer separation (Controllers, Services, Domain, Data, DTOs)
✅ **II. Type Safety First**: TypeScript strict mode, C# nullable refs, matching DTOs
✅ **III. Real-time by Design**: SignalR for chat, matches, notifications
✅ **IV. Location-Aware Architecture**: PostGIS with spatial indexes
✅ **V. Mobile-First Development**: React Native via Expo, swipe gestures
✅ **VI. One-Command Development**: Docker Compose orchestration

### Technical Standards Check

✅ **Backend Standards**: .NET 9, EF Core migrations, JWT auth, SignalR hubs
✅ **Frontend Standards**: TypeScript, Zustand, Axios interceptors
✅ **Infrastructure**: Docker Compose, PostgreSQL/PostGIS, health checks

### Quality Gates

✅ **Code Review**: Unit tests, DTOs, migrations, documentation
✅ **Testing**: Service tests, auth flow tests, multi-platform testing
✅ **Performance**: All targets defined and measurable

### Security Non-Negotiables

✅ All 8 security requirements addressed in design

### Developer Experience Non-Negotiables

✅ All 7 DX requirements maintained

**GATE RESULT**: ✅ PASSED - No violations

## Project Structure

### Documentation (this feature)

```
specs/001-tinder-clone-core/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
backend/
├── App/
│   ├── Controllers/     # API endpoints
│   │   ├── AuthController.cs
│   │   ├── ProfileController.cs
│   │   ├── FeedController.cs
│   │   ├── SwipeController.cs
│   │   ├── MatchController.cs
│   │   └── MessageController.cs
│   ├── Services/        # Business logic
│   │   ├── AuthService.cs
│   │   ├── ProfileService.cs
│   │   ├── FeedService.cs
│   │   ├── SwipeService.cs
│   │   ├── MatchService.cs
│   │   └── MessageService.cs
│   ├── Domain/          # Entity models
│   │   ├── User.cs
│   │   ├── Profile.cs
│   │   ├── Photo.cs
│   │   ├── Swipe.cs
│   │   ├── Match.cs
│   │   └── Message.cs
│   ├── Data/           # EF Core context
│   │   └── AppDbContext.cs
│   ├── DTOs/           # Request/Response models
│   │   ├── Auth/
│   │   ├── Profile/
│   │   ├── Feed/
│   │   ├── Swipe/
│   │   ├── Match/
│   │   └── Message/
│   ├── Hubs/           # SignalR
│   │   └── ChatHub.cs
│   └── Migrations/     # Database migrations
└── App.Tests/
    ├── Unit/
    ├── Integration/
    └── Fixtures/

frontend/
├── src/
│   ├── screens/        # React Native screens
│   │   ├── auth/
│   │   │   ├── LoginScreen.tsx
│   │   │   └── RegisterScreen.tsx
│   │   ├── profile/
│   │   │   ├── ProfileScreen.tsx
│   │   │   └── EditProfileScreen.tsx
│   │   ├── feed/
│   │   │   └── FeedScreen.tsx
│   │   ├── matches/
│   │   │   └── MatchesScreen.tsx
│   │   └── chat/
│   │       ├── ChatListScreen.tsx
│   │       └── ChatScreen.tsx
│   ├── services/       # API clients
│   │   ├── authService.ts
│   │   ├── profileService.ts
│   │   ├── feedService.ts
│   │   ├── swipeService.ts
│   │   ├── matchService.ts
│   │   ├── messageService.ts
│   │   └── signalrService.ts
│   ├── store/          # Zustand stores
│   │   ├── authStore.ts
│   │   ├── profileStore.ts
│   │   ├── feedStore.ts
│   │   ├── matchStore.ts
│   │   └── chatStore.ts
│   ├── components/     # Reusable components
│   │   ├── SwipeCard.tsx
│   │   ├── PhotoGallery.tsx
│   │   ├── MessageBubble.tsx
│   │   └── common/
│   ├── navigation/     # React Navigation setup
│   │   └── AppNavigator.tsx
│   ├── config/         # Configuration
│   │   └── api.ts
│   └── types/          # TypeScript definitions
│       └── index.ts
└── __tests__/
    ├── screens/
    ├── services/
    └── components/
```

**Structure Decision**: Monorepo structure with separate backend and frontend directories. Backend follows Clean Architecture with clear separation of concerns. Frontend follows React Native best practices with screens, services, and state management separation. This aligns with Constitution principles I (Clean Architecture) and V (Mobile-First Development).

## Complexity Tracking

*No violations - all design choices align with constitution principles*