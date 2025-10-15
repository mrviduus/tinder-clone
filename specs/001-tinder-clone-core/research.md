# Research: Tinder Clone Core

## Phase 0 Research Findings

### Testing Framework for React Native/Expo Frontend

**Decision**: Jest + React Native Testing Library
**Rationale**:
- Jest with jest-expo preset is the industry standard for Expo applications
- React Native Testing Library encourages user-centric testing patterns
- Full TypeScript support out of the box
- Active maintenance and compatibility with Expo SDK 51
- Replaces deprecated react-test-renderer for React 19+ compatibility

**Alternatives Considered**:
- Detox (E2E testing) - Overkill for unit/integration tests, better for E2E suite
- Enzyme - No longer maintained for React Native
- react-test-renderer - Deprecated in favor of React Native Testing Library

**Implementation Details**:
```json
{
  "devDependencies": {
    "jest-expo": "^51.0.0",
    "@testing-library/react-native": "^12.4.0",
    "@testing-library/jest-native": "^5.4.0",
    "ts-jest": "^29.1.0",
    "axios-mock-adapter": "^1.22.0"
  }
}
```

### Best Practices Identified

#### Backend Testing (.NET)
- xUnit for unit tests
- WebApplicationFactory for integration tests
- In-memory database for test isolation
- Fixture pattern for test data
- Moq for mocking dependencies

#### Frontend Testing (React Native)
- Component tests with React Native Testing Library
- Service tests with axios-mock-adapter
- Store tests with renderHook for Zustand
- Navigation tests with NavigationContainer wrapper
- Platform-specific tests using jest-expo presets

#### Performance Testing
- Load testing with k6 or JMeter for APIs
- React Native Performance Monitor for UI
- Lighthouse for web version
- Memory profiling with Expo tools

### SignalR Real-time Testing

**Decision**: Mock SignalR in unit tests, integration tests for real connections
**Rationale**:
- Unit tests should be fast and isolated
- Integration tests verify actual WebSocket behavior
- Mock connection state changes and message handlers

**Implementation**:
- Mock @microsoft/signalr module in Jest
- Test connection/disconnection logic
- Verify message handler registration
- Integration tests with actual SignalR server

### Geospatial Query Optimization

**Decision**: PostGIS geography type with spatial indexes
**Rationale**:
- Geography type provides accurate distance calculations on sphere
- Spatial indexes (GIST) enable efficient proximity queries
- ST_DWithin for radius searches
- Proper for global scale application

**Alternatives Considered**:
- Geometry type - Less accurate for large distances
- Haversine formula in application code - Inefficient for large datasets
- External geocoding service - Added latency and cost

**Query Pattern**:
```sql
SELECT * FROM profiles
WHERE ST_DWithin(
  location::geography,
  ST_MakePoint(longitude, latitude)::geography,
  radius_meters
)
ORDER BY ST_Distance(
  location::geography,
  ST_MakePoint(longitude, latitude)::geography
)
LIMIT 50;
```

### JWT Token Rotation Strategy

**Decision**: Sliding window refresh with token rotation
**Rationale**:
- 15-minute access tokens minimize exposure window
- 14-day refresh tokens balance security and UX
- Token rotation prevents replay attacks
- Automatic refresh in Axios interceptors

**Implementation**:
- Store refresh token family ID
- Invalidate entire family on suspicious activity
- Automatic retry with new token on 401
- Silent refresh before expiry

### Photo Storage Migration Path

**Decision**: Start with PostgreSQL BLOBs, prepare for cloud migration
**Rationale**:
- Simplifies MVP development and deployment
- No additional infrastructure initially
- Clear migration path to S3/Azure Blob Storage
- CDN integration ready

**Migration Preparation**:
- Abstract storage behind IPhotoService interface
- Store photo metadata separately from data
- Generate consistent URLs via service
- Prepare for async upload processing

### Docker Compose Orchestration

**Decision**: Multi-stage builds with health checks and dependencies
**Rationale**:
- Ensures correct startup order
- Automatic migration execution
- Health checks prevent premature connections
- Development and production configurations

**Key Features**:
- wait-for-it scripts for service dependencies
- Automatic database creation and seeding
- Volume mounts for hot reload
- Environment-specific overrides

### Mobile Platform Handling

**Decision**: Shared codebase with minimal platform-specific code
**Rationale**:
- Expo handles most platform differences
- Platform-specific styling via StyleSheet
- Conditional imports for iOS/Android features
- Web compatibility through Expo web

**Platform Considerations**:
- iOS: localhost for simulator, IP for device
- Android: 10.0.2.2 for emulator
- Web: Standard localhost
- Permissions handled via Expo APIs

## Conclusion

All technical clarifications have been resolved. The technology stack is well-defined:
- Backend: .NET 9 with xUnit testing
- Frontend: React Native/Expo with Jest + React Native Testing Library
- Database: PostgreSQL with PostGIS for spatial features
- Real-time: SignalR with proper testing strategies
- Infrastructure: Docker Compose with one-command setup

The research confirms alignment with all constitution principles and provides clear implementation patterns for each component.