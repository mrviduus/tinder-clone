# 🗄️ Database Architecture & Schema Documentation

## Overview
Complete database schema for the Tinder Clone application using PostgreSQL with PostGIS extension for geospatial features.

---

## 📊 Entity Relationship Diagram

```mermaid
erDiagram
    USERS ||--o| PROFILES : has
    USERS ||--o{ PHOTOS : uploads
    USERS ||--o{ SWIPES : makes
    USERS ||--o{ MATCHES : participates
    USERS ||--o{ MESSAGES : sends
    USERS ||--o{ REFRESH_TOKENS : owns
    MATCHES ||--o{ MESSAGES : contains
    PHOTOS }o--o{ MESSAGES : attached_to

    USERS {
        guid Id PK
        string Email UK
        string NormalizedEmail
        string UserName
        string NormalizedUserName
        string PasswordHash
        string SecurityStamp
        string ConcurrencyStamp
        string PhoneNumber
        boolean PhoneNumberConfirmed
        boolean TwoFactorEnabled
        datetime LockoutEnd
        boolean LockoutEnabled
        int AccessFailedCount
        boolean EmailConfirmed
    }

    PROFILES {
        guid UserId PK_FK
        string DisplayName
        datetime BirthDate
        int Gender
        string Bio
        string JobTitle
        string Company
        string School
        Point Location
        datetime CreatedAt
        datetime UpdatedAt
    }

    PHOTOS {
        guid Id PK
        guid UserId FK
        bytea PhotoData
        datetime UploadedAt
        boolean IsMain
        int DisplayOrder
    }

    SWIPES {
        guid Id PK
        guid SwiperId FK
        guid TargetUserId FK
        int Direction
        datetime SwipedAt
        Point SwipeLocation
    }

    MATCHES {
        guid Id PK
        guid UserAId FK
        guid UserBId FK
        datetime CreatedAt
        datetime UnmatchedAt
        guid UnmatchedBy
    }

    MESSAGES {
        guid Id PK
        guid MatchId FK
        guid SenderId FK
        string Content
        guid ImagePhotoId FK
        datetime SentAt
        datetime DeliveredAt
        datetime ReadAt
    }

    REFRESH_TOKENS {
        guid Id PK
        guid UserId FK
        string Token UK
        string JwtId
        datetime CreatedAt
        datetime ExpiresAt
        boolean IsUsed
        boolean IsInvalidated
        guid ReplacedByTokenId
    }
```

---

## 📋 Detailed Table Schemas

### 1. **users** (ASP.NET Identity)
Primary table for authentication and user management.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Primary key, unique user identifier |
| Email | `varchar(256)` | `NOT NULL, UNIQUE` | User's email address |
| NormalizedEmail | `varchar(256)` | `INDEX` | Uppercase email for searching |
| UserName | `varchar(256)` | | Username (often same as email) |
| NormalizedUserName | `varchar(256)` | `INDEX` | Uppercase username for searching |
| PasswordHash | `text` | | Hashed password |
| SecurityStamp | `text` | | Random value that changes when credentials change |
| ConcurrencyStamp | `text` | | Used for optimistic concurrency |
| PhoneNumber | `text` | | Optional phone number |
| PhoneNumberConfirmed | `boolean` | `DEFAULT false` | Phone verification status |
| TwoFactorEnabled | `boolean` | `DEFAULT false` | 2FA status |
| LockoutEnd | `timestamptz` | | Account lockout end time |
| LockoutEnabled | `boolean` | `DEFAULT true` | Whether lockout is enabled |
| AccessFailedCount | `int` | `DEFAULT 0` | Failed login attempts |
| EmailConfirmed | `boolean` | `DEFAULT false` | Email verification status |

**Indexes:**
- `IX_users_NormalizedEmail`
- `IX_users_NormalizedUserName`

---

### 2. **profiles**
Extended user information for dating profiles.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **UserId** | `uuid` | `PK, FK → users.Id` | Links to users table |
| DisplayName | `varchar(100)` | `NOT NULL` | Display name shown to others |
| BirthDate | `date` | `NOT NULL` | Date of birth |
| Gender | `int` | `NOT NULL` | 0=Unknown, 1=Male, 2=Female, 3=NonBinary |
| Bio | `text` | | Profile biography (max 500 chars) |
| JobTitle | `varchar(100)` | | Current job title |
| Company | `varchar(100)` | | Current company |
| School | `varchar(100)` | | Education institution |
| Location | `geography(Point)` | | PostGIS point for location |
| CreatedAt | `timestamptz` | `DEFAULT NOW()` | Profile creation timestamp |
| UpdatedAt | `timestamptz` | `DEFAULT NOW()` | Last update timestamp |

**Indexes:**
- `IX_profiles_Location` (GIST index for spatial queries)
- `IX_profiles_Gender`
- `IX_profiles_BirthDate`

**Constraints:**
- `CHECK (Age(BirthDate) >= 18)` - Must be 18 or older

---

### 3. **photos**
User uploaded photos for profiles.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Unique photo identifier |
| UserId | `uuid` | `FK → users.Id` | Photo owner |
| PhotoData | `bytea` | `NOT NULL` | Binary photo data (base64 encoded) |
| UploadedAt | `timestamptz` | `DEFAULT NOW()` | Upload timestamp |
| IsMain | `boolean` | `DEFAULT false` | Primary profile photo |
| DisplayOrder | `int` | `DEFAULT 0` | Photo display order |

**Indexes:**
- `IX_photos_UserId`
- `IX_photos_IsMain`

**Constraints:**
- `UNIQUE (UserId, IsMain) WHERE IsMain = true` - Only one main photo per user
- `CHECK (length(PhotoData) <= 5242880)` - Max 5MB file size

---

### 4. **swipes**
Records all swipe actions (likes/passes).

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Unique swipe identifier |
| SwiperId | `uuid` | `FK → users.Id` | User who swiped |
| TargetUserId | `uuid` | `FK → users.Id` | User being swiped on |
| Direction | `int` | `NOT NULL` | 0=Pass, 1=Like |
| SwipedAt | `timestamptz` | `DEFAULT NOW()` | Swipe timestamp |
| SwipeLocation | `geography(Point)` | | Location where swipe occurred |

**Indexes:**
- `IX_swipes_SwiperId_TargetUserId` (UNIQUE)
- `IX_swipes_SwiperId`
- `IX_swipes_TargetUserId`
- `IX_swipes_Direction`
- `IX_swipes_SwipedAt`

**Constraints:**
- `UNIQUE (SwiperId, TargetUserId)` - One swipe per user pair
- `CHECK (SwiperId != TargetUserId)` - Can't swipe on yourself

---

### 5. **matches**
Mutual likes that create matches.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Unique match identifier |
| UserAId | `uuid` | `FK → users.Id` | First user (lower ID) |
| UserBId | `uuid` | `FK → users.Id` | Second user (higher ID) |
| CreatedAt | `timestamptz` | `DEFAULT NOW()` | Match creation time |
| UnmatchedAt | `timestamptz` | | Unmatch timestamp if applicable |
| UnmatchedBy | `uuid` | `FK → users.Id` | User who unmatched |

**Indexes:**
- `IX_matches_UserAId_UserBId` (UNIQUE)
- `IX_matches_UserAId`
- `IX_matches_UserBId`
- `IX_matches_CreatedAt`

**Constraints:**
- `UNIQUE (UserAId, UserBId)` - One match per user pair
- `CHECK (UserAId < UserBId)` - Enforce ID ordering
- `CHECK (UserAId != UserBId)` - Can't match with yourself

**Business Logic:**
- UserA always has the smaller UUID
- Match created when both users like each other

---

### 6. **messages**
Chat messages between matched users.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Unique message identifier |
| MatchId | `uuid` | `FK → matches.Id` | Associated match |
| SenderId | `uuid` | `FK → users.Id` | Message sender |
| Content | `text` | | Message text content |
| ImagePhotoId | `uuid` | `FK → photos.Id` | Optional attached photo |
| SentAt | `timestamptz` | `DEFAULT NOW()` | Send timestamp |
| DeliveredAt | `timestamptz` | | Delivery timestamp |
| ReadAt | `timestamptz` | | Read timestamp |

**Indexes:**
- `IX_messages_MatchId_SentAt` (Composite for message ordering)
- `IX_messages_SenderId`
- `IX_messages_ReadAt`

**Constraints:**
- `CHECK (Content IS NOT NULL OR ImagePhotoId IS NOT NULL)` - Must have content or image
- `CHECK (length(Content) <= 1000)` - Max message length

---

### 7. **refresh_tokens**
JWT refresh tokens for authentication.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| **Id** | `uuid` | `PK` | Unique token identifier |
| UserId | `uuid` | `FK → users.Id` | Token owner |
| Token | `varchar(500)` | `UNIQUE` | Refresh token value |
| JwtId | `varchar(100)` | | Associated JWT ID |
| CreatedAt | `timestamptz` | `DEFAULT NOW()` | Token creation time |
| ExpiresAt | `timestamptz` | `NOT NULL` | Token expiration time |
| IsUsed | `boolean` | `DEFAULT false` | Whether token has been used |
| IsInvalidated | `boolean` | `DEFAULT false` | Whether token is invalidated |
| ReplacedByTokenId | `uuid` | `FK → refresh_tokens.Id` | New token if rotated |

**Indexes:**
- `IX_refresh_tokens_Token` (UNIQUE)
- `IX_refresh_tokens_UserId`
- `IX_refresh_tokens_ExpiresAt`

**Constraints:**
- `CHECK (ExpiresAt > CreatedAt)` - Expiry must be after creation

---

## 🔗 Relationships

### One-to-One
- `users` ↔ `profiles`: Each user has exactly one profile

### One-to-Many
- `users` → `photos`: User can have multiple photos
- `users` → `swipes`: User makes many swipes
- `users` → `messages`: User sends many messages
- `users` → `refresh_tokens`: User can have multiple tokens
- `matches` → `messages`: Match contains many messages

### Many-to-Many (via junction tables)
- `users` ↔ `users` (via `matches`): Users can match with each other
- `users` ↔ `users` (via `swipes`): Users swipe on each other

---

## 🚀 Database Optimizations

### Spatial Indexes (PostGIS)
```sql
-- For location-based user discovery
CREATE INDEX idx_profiles_location ON profiles USING GIST (Location);

-- Find users within radius
SELECT * FROM profiles
WHERE ST_DWithin(Location, ST_MakePoint(longitude, latitude)::geography, radius_meters);
```

### Performance Indexes
```sql
-- Message retrieval optimization
CREATE INDEX idx_messages_match_sent ON messages(match_id, sent_at DESC);

-- Swipe history lookup
CREATE INDEX idx_swipes_swiper_swiped ON swipes(swiper_id, swiped_at DESC);

-- Match lookup optimization
CREATE INDEX idx_matches_users ON matches(user_a_id, user_b_id);
```

### Partitioning Strategy (for scale)
```sql
-- Partition messages by month
CREATE TABLE messages_2025_01 PARTITION OF messages
FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

-- Partition swipes by user ID range
CREATE TABLE swipes_partition_1 PARTITION OF swipes
FOR VALUES WITH (modulus 4, remainder 0);
```

---

## 📈 Query Examples

### Find Potential Matches
```sql
-- Users not yet swiped on, within 10km, appropriate age/gender
SELECT p.*
FROM profiles p
WHERE p.user_id NOT IN (
    SELECT target_user_id FROM swipes WHERE swiper_id = :current_user_id
)
AND p.user_id != :current_user_id
AND ST_DWithin(p.Location, :user_location, 10000) -- 10km radius
AND p.gender = :preferred_gender
AND AGE(p.birth_date) BETWEEN :min_age AND :max_age
ORDER BY ST_Distance(p.Location, :user_location)
LIMIT 10;
```

### Get Match Conversations
```sql
-- Get all matches with last message
SELECT
    m.id as match_id,
    CASE
        WHEN m.user_a_id = :current_user_id THEN m.user_b_id
        ELSE m.user_a_id
    END as other_user_id,
    p.display_name,
    last_msg.content as last_message,
    last_msg.sent_at as last_message_at
FROM matches m
JOIN profiles p ON p.user_id = CASE
    WHEN m.user_a_id = :current_user_id THEN m.user_b_id
    ELSE m.user_a_id
END
LEFT JOIN LATERAL (
    SELECT content, sent_at
    FROM messages
    WHERE match_id = m.id
    ORDER BY sent_at DESC
    LIMIT 1
) last_msg ON true
WHERE (m.user_a_id = :current_user_id OR m.user_b_id = :current_user_id)
AND m.unmatched_at IS NULL
ORDER BY COALESCE(last_msg.sent_at, m.created_at) DESC;
```

### Create Match from Mutual Likes
```sql
-- Transaction to create match when both users like each other
BEGIN;

-- Check if target already liked current user
SELECT 1 FROM swipes
WHERE swiper_id = :target_user_id
AND target_user_id = :current_user_id
AND direction = 1; -- Like

-- If yes, create match
INSERT INTO matches (id, user_a_id, user_b_id, created_at)
VALUES (
    gen_random_uuid(),
    LEAST(:current_user_id, :target_user_id),
    GREATEST(:current_user_id, :target_user_id),
    NOW()
);

COMMIT;
```

---

## 🔒 Security Considerations

### Data Privacy
- Personal data encrypted at rest
- Location data stored as geography type (not exact coordinates)
- Photos stored as binary data (consider cloud storage for production)

### Access Control
```sql
-- Row Level Security for messages
CREATE POLICY messages_access ON messages
FOR ALL
TO authenticated_user
USING (
    sender_id = current_user_id() OR
    match_id IN (
        SELECT id FROM matches
        WHERE user_a_id = current_user_id()
        OR user_b_id = current_user_id()
    )
);
```

### Audit Trail
```sql
-- Audit table for sensitive operations
CREATE TABLE audit_log (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid REFERENCES users(id),
    action varchar(50),
    entity_type varchar(50),
    entity_id uuid,
    old_values jsonb,
    new_values jsonb,
    ip_address inet,
    user_agent text,
    created_at timestamptz DEFAULT NOW()
);
```

---

## 📊 Database Statistics

### Expected Data Volumes
- **Users**: 100K - 1M
- **Photos**: 3-6 per user (300K - 6M records)
- **Swipes**: 100-500 per user per day (10M - 500M records/year)
- **Matches**: 5-10% of swipes (500K - 50M records/year)
- **Messages**: 20-50 per match (10M - 2.5B records/year)

### Storage Requirements
- **Users + Profiles**: ~2KB per user = 2GB for 1M users
- **Photos**: ~500KB average = 3TB for 6M photos
- **Messages**: ~200 bytes per message = 500GB for 2.5B messages
- **Total**: ~4TB for 1M users with full activity

---

## 🔧 Maintenance Scripts

### Clean up expired tokens
```sql
DELETE FROM refresh_tokens
WHERE expires_at < NOW() - INTERVAL '30 days';
```

### Archive old messages
```sql
-- Move messages older than 1 year to archive table
INSERT INTO messages_archive
SELECT * FROM messages
WHERE sent_at < NOW() - INTERVAL '1 year';

DELETE FROM messages
WHERE sent_at < NOW() - INTERVAL '1 year';
```

### Update match statistics
```sql
-- Materialized view for match statistics
CREATE MATERIALIZED VIEW match_stats AS
SELECT
    user_id,
    COUNT(DISTINCT match_id) as total_matches,
    COUNT(DISTINCT CASE WHEN sent_at > NOW() - INTERVAL '7 days'
           THEN match_id END) as active_matches,
    AVG(message_count) as avg_messages_per_match
FROM (
    SELECT
        sender_id as user_id,
        match_id,
        sent_at,
        COUNT(*) as message_count
    FROM messages
    GROUP BY sender_id, match_id, sent_at
) t
GROUP BY user_id;

-- Refresh periodically
REFRESH MATERIALIZED VIEW CONCURRENTLY match_stats;
```

---

## 📝 Migration Notes

### Initial Setup
```sql
-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "postgis";

-- Create database
CREATE DATABASE tinder_clone
    WITH ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8';
```

### Version Control
All schema changes managed through Entity Framework Core migrations:
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

*Database Architecture v1.0.0 - Last Updated: September 2025*