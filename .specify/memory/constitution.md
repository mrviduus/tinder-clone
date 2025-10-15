# Tinder Clone Constitution

## Core Principles

### I. Clean Architecture
Every layer has a single responsibility and clear boundaries:
- Controllers handle HTTP requests and responses only
- Services contain all business logic and orchestration
- Domain models represent core entities with relationships
- Data layer manages persistence through Entity Framework Core
- DTOs define API contracts, never expose domain models directly

### II. Type Safety First
Leverage strong typing across the entire stack:
- TypeScript strict mode in frontend with no `any` types
- C# nullable reference types enabled in backend
- DTOs must match exactly between frontend and backend
- Validation at compile-time wherever possible
- Runtime validation as a safety net, not primary defense

### III. Real-time by Design
Build for instantaneous user interactions:
- SignalR for all real-time features (chat, matches, notifications)
- No polling - push updates immediately
- Optimistic UI updates with rollback on failure
- Connection resilience with automatic reconnection
- Offline queue for actions, sync when online

### IV. Location-Aware Architecture
Spatial data is a first-class citizen:
- PostGIS for all geospatial operations
- Geography type for accurate distance calculations
- Spatial indexes on all location queries
- Proximity matching as core feature
- Privacy-first location handling

### V. Mobile-First Development
Design for mobile, adapt for web:
- React Native as primary platform via Expo
- Native gestures (swipe) for core interactions
- Platform-specific code when necessary
- Responsive design for web compatibility
- Performance optimization for mobile constraints

### VI. One-Command Development
Zero-friction development environment setup:
- Single `docker-compose up` starts entire stack
- Automatic database creation and migration on startup
- Backend API starts with proper configuration
- Frontend launches with hot-reload enabled
- All services interconnected and ready to use
- No manual setup steps or configuration required
- New developers productive in under 5 minutes

## Technical Standards

### Backend Standards (.NET 9)
**API Design:**
- RESTful endpoints with consistent naming
- Swagger/OpenAPI documentation required
- Request/Response DTOs for all endpoints
- HTTP status codes follow RFC standards
- Versioning strategy prepared (not yet implemented)

**Data Management:**
- Entity Framework Core with code-first migrations
- PostgreSQL with PostGIS extension
- All timestamps stored as UTC
- Soft deletes for critical data (users, matches)
- Transaction boundaries at service layer

**Authentication & Security:**
- JWT with 15-minute access tokens
- Refresh tokens with 14-day expiry and rotation
- ASP.NET Identity for user management
- Password complexity requirements enforced
- CORS explicitly configured per environment

**Real-time Communication:**
- SignalR hubs for WebSocket connections
- Group-based messaging (one group per match)
- Typed hubs with strong contracts
- Automatic reconnection handling
- Authentication required for all hubs

### Frontend Standards (React Native/Expo)
**Architecture:**
- Screens handle UI and user interaction
- Services manage API communication
- Zustand for global state management
- Navigation with React Navigation
- Config centralized in dedicated files

**Code Quality:**
- TypeScript with strict configuration
- No use of `any` type
- Interfaces for all API responses
- Platform-specific code clearly marked
- Error boundaries for graceful failures

**API Integration:**
- Axios with request/response interceptors
- Automatic token refresh on 401
- Retry logic for transient failures
- Request queuing when offline
- Proper error messages for users

### Infrastructure Standards
**Development Environment:**
- Docker Compose orchestrates entire stack
- Single command startup: `docker-compose up`
- Automatic migrations run on container start
- Environment variables via .env files (with .env.example template)
- Consistent ports across team (8080, 19006, 5432)
- Health checks verify service readiness
- Wait-for scripts ensure proper startup order
- Structured logging with Serilog

**Database:**
- PostgreSQL 16 with PostGIS 3.4
- Migrations checked into source control
- Test data via dedicated migrations
- Indexes on all foreign keys and search fields
- Geography type for location data

## Quality Gates

### Code Review Requirements
All code must pass these checks before merge:
- No compiler warnings
- No linting errors
- Services have unit tests
- API changes have updated DTOs
- Migrations are reversible
- Documentation updated if needed

### Testing Standards
**Required Tests:**
- Unit tests for all service methods
- Integration tests for auth flow
- API endpoint tests for new controllers
- Manual testing on iOS, Android, and Web
- Test with multiple user accounts

**Test Data:**
- 8 test users with complete profiles
- Password: `Password123!` for all
- Located in San Francisco area
- Mix of genders and preferences
- Photos and messages pre-populated

### Performance Requirements
**Response Times:**
- Feed load: < 500ms
- Swipe action: < 200ms
- Match calculation: < 100ms
- Message delivery: < 50ms
- Photo upload: < 5 seconds

**Resource Limits:**
- Photo size: 5MB maximum
- API payload: 10MB maximum
- Concurrent connections: Design for 1000+
- Database connections: Pooled and limited
- Memory usage: Monitor for leaks

## Development Workflow

### Local Development Setup
**Getting Started (MUST be this simple):**
```bash
git clone <repository>
cd tinder-clone
cp .env.example .env
docker-compose up
```
**That's it!** No additional steps required. The system will:
- Create and configure the database
- Run all migrations including test data
- Start the backend API on port 8080
- Launch the frontend on port 19006
- Configure all service connections
- Be ready for development in under 5 minutes

**Maintaining One-Command Simplicity:**
- All new dependencies must be added to Docker images
- Migrations must be idempotent and auto-run
- Configuration must have sensible defaults
- Breaking changes require docker-compose updates
- Document only if one-command setup changes

### Git Strategy
**Branch Management:**
- `main` branch is always deployable
- Feature branches from main
- Branch names: `feature/description`
- Squash merge to main
- Delete branches after merge

**Commit Standards:**
- Descriptive commit messages
- Present tense ("Add feature" not "Added feature")
- Reference issue numbers when applicable
- Atomic commits (one logical change)

### Deployment Readiness
**Pre-Production Checklist:**
- All migrations tested and reversible
- Environment variables documented
- Health checks passing
- Logging configured and tested
- HTTPS configured (production only)

**Production Requirements:**
- JWT secret key rotated
- CORS configured for production domains
- Rate limiting enabled
- SSL certificates valid
- Monitoring and alerting configured

## Platform-Specific Guidelines

### iOS Development
**Configuration:**
- Backend must bind to all interfaces (`http://+:8080`)
- Simulator uses `localhost:8080`
- Device uses Mac's IP address
- AsyncStorage for token persistence
- Handle iOS-specific permissions

**Testing:**
- Test on latest iOS version
- Verify on iPhone and iPad
- Check different screen sizes
- Validate gesture recognition
- Test background/foreground transitions

### Android Development
**Configuration:**
- Emulator uses `10.0.2.2:8080`
- Device uses machine IP or ngrok
- Handle Android permissions explicitly
- Test back button behavior
- Validate deep linking

**Testing:**
- Test on API 21+ (Android 5.0+)
- Check different screen densities
- Verify on phones and tablets
- Test orientation changes
- Validate notification handling

### Web Development
**Compatibility:**
- Support latest 2 versions of major browsers
- Responsive design for all screen sizes
- Keyboard navigation support
- Handle browser storage limits
- Graceful degradation for unsupported features

## Security Non-Negotiables

1. **Never store tokens in plain text** - Use secure storage
2. **Never trust client input** - Validate everything server-side
3. **Never skip authentication** - Every protected endpoint must validate
4. **Never log sensitive data** - No passwords, tokens, or PII in logs
5. **Never use production data locally** - Anonymize if needed
6. **Always use HTTPS in production** - No exceptions
7. **Always rotate secrets** - Refresh tokens must rotate
8. **Always validate file uploads** - Check type, size, and content

## Developer Experience Non-Negotiables

1. **Never break one-command startup** - `docker-compose up` must always work
2. **Never require manual database setup** - Migrations run automatically
3. **Never require manual configuration** - Defaults must work out of the box
4. **Never add undocumented dependencies** - Update Docker images immediately
5. **Always maintain hot-reload** - Development must be iterative
6. **Always include test data** - New developers need immediate feedback
7. **Always test docker-compose changes** - Clean environment test required

## Future Evolution

### Scaling Preparation
**Near-term (< 1000 users):**
- Current architecture sufficient
- Monitor performance metrics
- Optimize slow queries
- Cache frequently accessed data

**Mid-term (1000-10000 users):**
- Redis for SignalR backplane
- Move photos to cloud storage (S3/Azure)
- Implement CDN for static assets
- Database read replicas

**Long-term (10000+ users):**
- Microservices architecture
- Message queue for async operations
- Elasticsearch for complex searches
- Kubernetes for orchestration

### Feature Roadmap Preparation
**Payment Integration:**
- Premium features architecture
- Stripe/PayPal integration ready
- Subscription management design

**Enhanced Matching:**
- ML-ready data structure
- Recommendation engine hooks
- Behavioral analytics foundation

**Social Features:**
- Group chat architecture
- Event system design
- Social graph structure

## Amendments

### Amendment Process
1. Propose change with justification
2. Team review and discussion
3. Update constitution if approved
4. Update dependent artifacts
5. Communicate changes to team

### Version History
- **1.0.0** - Initial constitution (2024-01-14)
  - Established core principles and standards
  - Defined quality gates and workflow
  - Set security requirements

- **1.1.0** - One-Command Development (2024-01-14)
  - Added Principle VI: One-Command Development
  - Enhanced infrastructure standards for automated setup
  - Added Developer Experience Non-Negotiables
  - Emphasized zero-friction onboarding

**Version**: 1.1.0 | **Ratified**: 2024-01-14 | **Last Amended**: 2024-01-14