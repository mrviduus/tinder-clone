# 🎨 Database Visual Architecture Diagrams

## Complete System ER Diagram

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│                          TINDER CLONE DATABASE SCHEMA                             │
├──────────────────────────────────────────────────────────────────────────────────┤
│                                                                                    │
│  ┌─────────────────┐                        ┌──────────────────┐                 │
│  │     USERS       │                        │    PROFILES     │                 │
│  ├─────────────────┤                        ├──────────────────┤                 │
│  │ PK: Id (uuid)   │──────────1:1──────────▶│ PK,FK: UserId   │                 │
│  │ Email           │                        │ DisplayName      │                 │
│  │ PasswordHash    │                        │ BirthDate        │                 │
│  │ EmailConfirmed  │                        │ Gender           │                 │
│  │ PhoneNumber     │                        │ Bio              │                 │
│  │ LockoutEnd      │                        │ Location (Point) │                 │
│  └────────┬────────┘                        │ JobTitle         │                 │
│           │                                  │ Company          │                 │
│           │                                  │ School           │                 │
│           │                                  └──────────────────┘                 │
│           │                                                                        │
│           │         ┌──────────────────┐         ┌──────────────────┐            │
│           ├────1:N──▶     PHOTOS       │         │     SWIPES       │◀───┐       │
│           │         ├──────────────────┤         ├──────────────────┤    │       │
│           │         │ PK: Id           │         │ PK: Id           │    │       │
│           │         │ FK: UserId       │         │ FK: SwiperId     │────┤       │
│           │         │ PhotoData (blob) │         │ FK: TargetUserId │    │       │
│           │         │ UploadedAt       │         │ Direction (0/1)  │    │       │
│           │         │ IsMain           │         │ SwipedAt         │    │       │
│           │         │ DisplayOrder     │         │ SwipeLocation    │    │       │
│           │         └───────┬──────────┘         └──────────────────┘    │       │
│           │                 │                                             │       │
│           │                 │ N:1                                         │       │
│           │                 ▼                                             │       │
│           │         ┌──────────────────┐         ┌──────────────────┐    │       │
│           ├────1:N──▶    MESSAGES      │◀───N:1──│     MATCHES      │    │       │
│           │         ├──────────────────┤         ├──────────────────┤    │       │
│           │         │ PK: Id           │         │ PK: Id           │    │       │
│           │         │ FK: MatchId      │         │ FK: UserAId      │────┼───M:N │
│           │         │ FK: SenderId     │         │ FK: UserBId      │────┘       │
│           │         │ FK: ImagePhotoId │         │ CreatedAt        │            │
│           │         │ Content          │         │ UnmatchedAt      │            │
│           │         │ SentAt           │         │ UnmatchedBy      │            │
│           │         │ DeliveredAt      │         └──────────────────┘            │
│           │         │ ReadAt           │                                         │
│           │         └──────────────────┘                                         │
│           │                                                                       │
│           │         ┌──────────────────┐                                        │
│           └────1:N──▶ REFRESH_TOKENS   │                                        │
│                     ├──────────────────┤                                        │
│                     │ PK: Id           │                                        │
│                     │ FK: UserId       │                                        │
│                     │ Token (unique)   │                                        │
│                     │ JwtId            │                                        │
│                     │ ExpiresAt        │                                        │
│                     │ IsUsed           │                                        │
│                     │ IsInvalidated    │                                        │
│                     └──────────────────┘                                        │
│                                                                                  │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Detailed Table Relationships

### User-Centric View
```
                              USER (Alice)
                                   │
        ┌──────────┬───────────┬──┴──┬──────────┬──────────┐
        ▼          ▼           ▼     ▼          ▼          ▼
    PROFILE    PHOTOS(6)   SWIPES  MATCHES   MESSAGES  TOKENS(2)
        │          │           │       │         │          │
    Name: Alice    │      Bob: Like    │     "Hi Bob!"     │
    Age: 28        │      Carl: Pass   │                   │
    Bio: "..."     │      Dan: Like    └─────Match────┐    │
                   │                          with     │    │
              Main Photo                      Bob      │    │
              Photo 2...                       │       │    │
                                               ▼       ▼    ▼
                                           MESSAGES  in  MATCH
```

### Matching Flow Diagram
```
     Alice                           Bob
       │                              │
       ▼                              ▼
   SWIPES TABLE                  SWIPES TABLE
   ┌───────────┐                ┌───────────┐
   │Alice→Bob:❤│                │Bob→Alice:?│
   └───────────┘                └───────────┘
                     ⬇
                Time passes
                     ⬇
   ┌───────────┐                ┌───────────┐
   │Alice→Bob:❤│                │Bob→Alice:❤│
   └───────────┘                └───────────┘
                     ⬇
              🎉 MUTUAL LIKE 🎉
                     ⬇
                MATCHES TABLE
              ┌──────────────┐
              │  Match Created │
              │ UserA: Alice  │
              │ UserB: Bob    │
              │ CreatedAt: Now│
              └──────────────┘
                     ⬇
              Chat Unlocked!
                     ⬇
               MESSAGES TABLE
```

## Data Flow Architecture

### 1. User Registration Flow
```
    NEW USER SIGNUP
          │
          ▼
    ┌─────────────┐
    │Create USER  │──────────────┐
    │Record       │              │
    └─────────────┘              ▼
          │                ┌─────────────┐
          ▼                │Create       │
    ┌─────────────┐        │REFRESH_TOKEN│
    │Create       │        └─────────────┘
    │PROFILE      │
    └─────────────┘
          │
          ▼
    ┌─────────────┐
    │Upload PHOTOS│
    │(Optional)   │
    └─────────────┘
```

### 2. Swipe & Match Flow
```
    USER SWIPES RIGHT
          │
          ▼
    ┌─────────────┐
    │Record SWIPE │
    │Direction: 1 │
    └─────────────┘
          │
          ▼
    Check Mutual?
          │
    ┌─────┴─────┐
    NO          YES
    │            │
    ▼            ▼
   END    ┌─────────────┐
          │Create MATCH │
          └─────────────┘
                 │
                 ▼
          ┌─────────────┐
          │Notify Users │
          │Enable Chat  │
          └─────────────┘
```

### 3. Messaging Flow
```
    USER SENDS MESSAGE
          │
          ▼
    ┌─────────────┐
    │Validate     │
    │Match Exists │
    └─────────────┘
          │
          ▼
    ┌─────────────┐
    │Save to      │
    │MESSAGES     │
    └─────────────┘
          │
          ▼
    ┌─────────────┐
    │Broadcast via│
    │SignalR Hub  │
    └─────────────┘
          │
    ┌─────┴─────┐
    ▼           ▼
  Sender    Recipient
  (Echo)    (Deliver)
```

## Index Strategy Visualization

### Spatial Index (Location-based Discovery)
```
                 LOCATION INDEX (PostGIS GIST)
                           │
    ┌──────────────────────┼──────────────────────┐
    │                      │                      │
    ▼                      ▼                      ▼
  ZONE A                 ZONE B                 ZONE C
  Users: 1-1000         Users: 1001-2000      Users: 2001-3000
    │                      │                      │
    ├─ Alice (x,y)        ├─ Charlie            ├─ Eve
    ├─ Bob (x,y)          ├─ Diana              └─ Frank
    └─ ...                └─ ...

Query: Find users within 10km of point (lat, long)
Result: Efficiently returns nearby users using spatial index
```

### Message Index (Conversation Performance)
```
           COMPOSITE INDEX: (match_id, sent_at DESC)
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                 │
    Match_001         Match_002         Match_003
         │                 │                 │
    ┌────┴────┐      ┌────┴────┐      ┌────┴────┐
    │Messages │      │Messages │      │Messages │
    │Sorted by│      │Sorted by│      │Sorted by│
    │sent_at  │      │sent_at  │      │sent_at  │
    └─────────┘      └─────────┘      └─────────┘

Query: Get last 30 messages for match_id
Result: O(log n) retrieval using index
```

## Constraint Enforcement Diagram

### Unique Constraints
```
        SWIPES TABLE                      MATCHES TABLE
   ┌───────────────────┐            ┌───────────────────┐
   │ UNIQUE:           │            │ UNIQUE:           │
   │ (SwiperId,        │            │ (UserAId,         │
   │  TargetUserId)    │            │  UserBId)         │
   └───────────────────┘            │                   │
           │                        │ WHERE:            │
           ▼                        │ UserAId < UserBId│
   ❌ Can't swipe twice             └───────────────────┘
   on same person                            │
                                             ▼
                                    ❌ One match per pair
                                    (regardless of order)
```

### Check Constraints
```
   PROFILES                    MESSAGES                  MATCHES
      │                           │                         │
      ▼                           ▼                         ▼
  Age >= 18                 Content OR Photo          UserA != UserB
  (BirthDate check)         must exist                (No self-match)
```

## Security & Performance Layers

### Row-Level Security
```
                    USER REQUEST
                         │
                    ┌────▼────┐
                    │   JWT    │
                    │  Token   │
                    └────┬────┘
                         │
                ┌────────▼────────┐
                │  Authorization  │
                │     Layer       │
                └────────┬────────┘
                         │
            ┌────────────┼────────────┐
            ▼            ▼            ▼
        Own Profile  Own Matches  Own Messages
         Only         Only          Only
```

### Caching Strategy
```
    REQUEST
        │
        ▼
   ┌─────────┐
   │ Cache?  │──────Yes──────▶ Return Cached
   └─────────┘
        │
        No
        ▼
   ┌─────────┐
   │Database │
   │ Query   │
   └─────────┘
        │
        ▼
   Update Cache
        │
        ▼
    Return Data

Cache Layers:
1. User Profiles (5 min TTL)
2. Match Lists (1 min TTL)
3. Recent Messages (30 sec TTL)
```

## Scale Considerations

### Data Growth Projection
```
Users Timeline:
Day 1:     100 users     →  600 photos    →  5K swipes
Month 1:   10K users     →  60K photos   →  1M swipes
Year 1:    100K users    →  600K photos  →  36M swipes
Year 2:    1M users      →  6M photos    →  365M swipes

Storage Growth:
├── Users/Profiles: 2GB
├── Photos: 3TB (500KB avg)
├── Messages: 500GB
└── Total: ~4TB
```

### Partitioning Strategy
```
                MESSAGES TABLE
                      │
    ┌─────────────────┼─────────────────┐
    │                 │                 │
messages_2025_01  messages_2025_02  messages_2025_03
(January)         (February)        (March)
    │                 │                 │
  1.2M msgs         980K msgs        1.5M msgs

Benefits:
- Faster queries on recent data
- Easier archival of old data
- Parallel query execution
```

---

*Visual Database Architecture Documentation v1.0.0*
*Diagrams represent PostgreSQL 15+ with PostGIS 3.3+*