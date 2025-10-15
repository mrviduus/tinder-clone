# Tinder Clone - Comprehensive Testing Plan & Issue Report

## Date: 2025-01-21
## Author: Senior Developer/Architect Review

## 🚨 Critical Issues Found

### 1. Database & Infrastructure
- **Issue**: Docker not running - Required for PostgreSQL with PostGIS
- **Fix**: User needs to start Docker Desktop
- **Issue**: Project targets .NET 8 but only .NET 9 is installed
- **Fix**: Either install .NET 8 SDK or update project to .NET 9

### 2. Test Data Issues
- **Issue**: Only 2 test users (Alice, Bob) in migrations, README mentions 5 users
- **Fix**: Created comprehensive SQL script with 8 test users
- **Location**: `/backend/App/Migrations/AddCompleteTestData.sql`

## 📋 Testing Checklist

### Phase 1: Infrastructure Setup
- [ ] Start Docker Desktop
- [ ] Run `docker-compose up -d db` to start PostgreSQL
- [ ] Apply existing migrations: `cd backend/App && dotnet ef database update`
- [ ] Apply comprehensive test data: `psql -h localhost -U appuser -d appdb -f backend/App/Migrations/AddCompleteTestData.sql`

### Phase 2: Backend Testing

#### 2.1 Registration Flow
```bash
# Test registration endpoint
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "TestPass123!",
    "displayName": "Test User",
    "birthDate": "1995-01-01",
    "gender": 1
  }'
```

**Expected**:
- ✅ User created in `users` table
- ✅ Profile created in `profiles` table
- ✅ Returns UserId

**Validation Points**:
- Age validation (must be 18+)
- Email uniqueness
- Password complexity (min 8 chars)

#### 2.2 Login Flow
```bash
# Test login endpoint
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alice@example.com",
    "password": "Password123!"
  }'
```

**Expected**:
- ✅ Returns access token (15 min expiry)
- ✅ Returns refresh token (14 day expiry)
- ✅ Tokens are valid JWT

#### 2.3 Profile Management
```bash
# Get current profile (requires auth token)
curl -X GET http://localhost:8080/api/profile \
  -H "Authorization: Bearer <access_token>"

# Update profile
curl -X PUT http://localhost:8080/api/profile \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "bio": "Updated bio",
    "searchGender": 2,
    "ageMin": 25,
    "ageMax": 35,
    "maxDistanceKm": 50
  }'
```

### Phase 3: Frontend Testing

#### 3.1 Registration Form
1. Navigate to registration screen
2. Test validation:
   - [ ] Empty fields show error
   - [ ] Password < 8 chars shows error
   - [ ] Invalid email format shows error
   - [ ] Birth date validation (18+ years)
   - [ ] Gender selection required

3. Test successful registration:
   - [ ] Fill all fields correctly
   - [ ] Submit form
   - [ ] Auto-login after registration
   - [ ] Navigate to main screen

#### 3.2 Login Form
1. Test with existing users:
   - [ ] alice@example.com / Password123!
   - [ ] bob@example.com / Password123!

2. Test error cases:
   - [ ] Wrong password
   - [ ] Non-existent email
   - [ ] Token storage in localStorage

### Phase 4: Matching System Testing

#### 4.1 Feed Discovery
```bash
# Get feed of potential matches
curl -X GET http://localhost:8080/api/feed \
  -H "Authorization: Bearer <access_token>"
```

**Validation**:
- [ ] Returns users within distance range
- [ ] Filters by gender preferences
- [ ] Excludes already swiped users
- [ ] Excludes blocked users

#### 4.2 Swipe Testing
1. Login as Alice
2. Swipe on available profiles:
   - [ ] Right swipe (like) works
   - [ ] Left swipe (pass) works
   - [ ] Swipe is recorded in database

3. Test mutual like (match creation):
   - [ ] Alice likes Charlie
   - [ ] Login as Charlie
   - [ ] Charlie likes Alice
   - [ ] Match is created automatically

#### 4.3 Match Verification
```bash
# Get matches
curl -X GET http://localhost:8080/api/matches \
  -H "Authorization: Bearer <access_token>"
```

**Expected**:
- [ ] Shows all mutual likes
- [ ] Includes match timestamp
- [ ] Shows other user's profile info

### Phase 5: Chat System Testing

#### 5.1 SignalR Connection
1. Open browser console
2. Check for SignalR connection:
   - [ ] Connection established
   - [ ] Authentication successful
   - [ ] Joins match groups

#### 5.2 Message Flow
1. Open chat with matched user:
   - [ ] Can send messages
   - [ ] Messages appear in real-time
   - [ ] Typing indicators work
   - [ ] Read receipts update

2. Test edge cases:
   - [ ] Cannot message non-matched users
   - [ ] Messages persist after refresh
   - [ ] Offline messages delivered when online

### Phase 6: Advanced Features

#### 6.1 Photo Management
- [ ] Upload profile photo
- [ ] Set primary photo
- [ ] Delete photo
- [ ] Photo size validation (5MB max)

#### 6.2 Location Services
- [ ] Update location
- [ ] Distance calculation works
- [ ] Feed respects distance preferences

#### 6.3 Unmatch Feature
- [ ] Unmatch button in chat
- [ ] Confirmation dialog
- [ ] Removes match and all messages
- [ ] Both users affected

## 🧪 Test Users Reference

| Email | Password | Name | Age | Gender | Seeks | Location |
|-------|----------|------|-----|--------|-------|----------|
| alice@example.com | Password123! | Alice | 25 | Female | Male | NYC |
| bob@example.com | Password123! | Bob | 28 | Male | Female | NYC |
| charlie@example.com | Password123! | Charlie | 30 | Male | Female | Manhattan |
| diana@example.com | Password123! | Diana | 26 | Female | Male | Upper East Side |
| eve@example.com | Password123! | Eve | 24 | Female | Male | Chelsea |
| frank@example.com | Password123! | Frank | 27 | Male | Female | East Village |
| grace@example.com | Password123! | Grace | 29 | Female | Male | Midtown |
| henry@example.com | Password123! | Henry | 32 | Male | Female | Brooklyn |

**Pre-configured Relationships**:
- Alice ↔️ Bob: Matched with chat history
- Charlie → Diana: One-way like
- Eve → Charlie: Pass
- Frank → Eve: One-way like

## 🛠️ Quick Fix Commands

```bash
# Start all services
docker-compose up --build

# Apply test data
docker exec -it tinder-clone-db-1 psql -U appuser -d appdb -f /Migrations/AddCompleteTestData.sql

# View logs
docker-compose logs -f api

# Reset database
docker-compose down -v
docker-compose up -d db
cd backend/App && dotnet ef database update

# Run backend locally
cd backend/App
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Run frontend locally
cd frontend
npm install
npm run web
```

## 📊 Success Criteria

The application is considered working when:

1. **Registration**: New users can register and auto-login
2. **Profiles**: Users can view/edit profiles with photos
3. **Discovery**: Feed shows appropriate candidates by location/preferences
4. **Matching**: Mutual likes create matches instantly
5. **Chat**: Real-time messaging works between matched users
6. **Performance**: Actions complete within 2 seconds
7. **Error Handling**: All errors show user-friendly messages

## 🐛 Known Issues to Fix

1. **Frontend Date Picker**: Birth date input is text field, needs proper date picker
2. **Photo Placeholders**: All test users have 1x1 pixel placeholders, need real test images
3. **Location Permission**: Frontend doesn't request location permission properly
4. **Token Refresh**: Auto-refresh not tested thoroughly
5. **Offline Handling**: App doesn't handle offline state gracefully

## 📝 Next Steps

1. Fix .NET version compatibility issue
2. Apply comprehensive test data
3. Run through complete testing checklist
4. Fix identified bugs
5. Add integration tests
6. Performance testing with multiple concurrent users
7. Security audit (JWT, SQL injection, XSS)

## 🚀 Deployment Readiness

Before production deployment:
- [ ] Change JWT secret key
- [ ] Enable HTTPS
- [ ] Move photos to CDN/S3
- [ ] Add rate limiting
- [ ] Implement email verification
- [ ] Add push notifications
- [ ] Set up monitoring/logging
- [ ] Database backups
- [ ] Load testing completed