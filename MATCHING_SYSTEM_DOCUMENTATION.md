# Tinder Clone - Matching System Documentation

## Overview
This document describes the complete matching and chat system implementation for the Tinder clone application. The system follows Tinder's matching logic where two users must mutually "like" each other to create a match and enable chat functionality.

## System Architecture

### Backend Components

#### Domain Models
- **Swipe**: Records user swipe actions (like/pass)
- **Match**: Created when two users mutually like each other
- **Message**: Chat messages between matched users
- **User/Profile**: User information and profile data
- **Photo**: User photos with base64 encoded data

#### Services
1. **SwipeService** (`/backend/App/Services/SwipeService.cs`)
   - Processes swipe actions (like/pass)
   - Checks for mutual likes
   - Creates matches when mutual likes occur
   - Prevents duplicate swipes (idempotency)

2. **MatchService** (`/backend/App/Services/MatchService.cs`)
   - Retrieves user matches
   - Provides match details
   - Handles unmatch functionality
   - Verifies user participation in matches

3. **MessageService** (`/backend/App/Services/MessageService.cs`)
   - Sends messages between matched users
   - Retrieves message history
   - Marks messages as read
   - Validates user permissions

4. **FeedService** (`/backend/App/Services/FeedService.cs`)
   - Provides candidate profiles for swiping
   - Filters out already swiped users
   - Filters out matched users

#### Controllers
1. **SwipesController** (`/api/swipes`)
   - `POST /api/swipes` - Process a swipe action

2. **MatchesController** (`/api/matches`)
   - `GET /api/matches` - Get all matches for current user
   - `GET /api/matches/{matchId}` - Get match details
   - `DELETE /api/matches/{matchId}` - Unmatch
   - `GET /api/matches/{matchId}/messages` - Get messages
   - `POST /api/matches/{matchId}/messages` - Send message

3. **FeedController** (`/api/feed`)
   - `GET /api/feed/candidates` - Get swipeable profiles

#### Real-time Communication
**ChatHub** (`/backend/App/Hubs/ChatHub.cs`)
- SignalR hub for real-time messaging
- Methods:
  - `JoinMatch(matchId)` - Join match chat room
  - `SendMessage(request)` - Send real-time message
  - `Typing(request)` - Send typing indicators
  - `MarkRead(request)` - Send read receipts

### Frontend Components

#### Screens
1. **Home Screen** (`/frontend/screens/Home.tsx`)
   - Displays swipeable card stack
   - Handles swipe gestures (left = pass, right = like)
   - Shows match notifications
   - Auto-loads more candidates

2. **Matches Screen** (`/frontend/screens/Matches.tsx`)
   - Lists all matches
   - Shows last message preview
   - Displays unread message count
   - Pull-to-refresh functionality

3. **Messages Screen** (`/frontend/screens/Messages.tsx`)
   - Real-time chat interface
   - Typing indicators
   - Read receipts
   - Unmatch button
   - Message history with pagination

#### Services
1. **SwipeService** (`/frontend/src/services/swipeService.ts`)
   - Processes swipe actions
   - Returns match status

2. **MatchService** (`/frontend/src/services/matchService.ts`)
   - Fetches matches list
   - Gets match details
   - Handles unmatch action

3. **MessageService** (`/frontend/src/services/messageService.ts`)
   - Sends messages
   - Retrieves message history
   - Marks messages as read

4. **SignalRService** (`/frontend/src/services/signalrService.ts`)
   - Manages WebSocket connection
   - Handles real-time events
   - Manages typing indicators

## Matching Flow

### 1. Swipe Process
```
User A swipes on User B
├── If LIKE:
│   ├── Check if User B already liked User A
│   │   ├── YES: Create Match
│   │   └── NO: Store swipe, wait for User B
│   └── Return match status
└── If PASS:
    └── Store swipe, no match possible
```

### 2. Match Creation
When a mutual like occurs:
1. Match record created with both user IDs
2. Both users can now see each other in matches list
3. Chat functionality enabled between users
4. Match notification displayed

### 3. Chat Flow
```
Match exists between User A and User B
├── User A sends message
│   ├── Validate match participation
│   ├── Store message in database
│   └── Broadcast via SignalR to User B
└── User B receives real-time update
    └── Updates UI with new message
```

### 4. Unmatch Process
When a user unmatches:
1. All messages deleted
2. Match record removed
3. Original swipes deleted
4. Users can swipe on each other again

## API Endpoints

### Swipe Endpoint
```http
POST /api/swipes
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetUserId": "guid",
  "direction": "like" | "pass"
}

Response:
{
  "matched": boolean,
  "matchId": "guid" | null
}
```

### Get Matches
```http
GET /api/matches?page=1&pageSize=50
Authorization: Bearer {token}

Response:
[
  {
    "matchId": "guid",
    "counterpart": {
      "userId": "guid",
      "displayName": "string",
      "photos": [...]
    },
    "lastMessagePreview": "string",
    "unreadCount": number,
    "createdAt": "datetime"
  }
]
```

### Send Message
```http
POST /api/matches/{matchId}/messages
Authorization: Bearer {token}
Content-Type: application/json

{
  "matchId": "guid",
  "text": "string",
  "photoId": "guid" | null
}

Response:
{
  "id": "guid",
  "matchId": "guid",
  "senderId": "guid",
  "content": "string",
  "imagePhotoId": "guid" | null,
  "sentAt": "datetime",
  "deliveredAt": "datetime" | null,
  "readAt": "datetime" | null
}
```

### Unmatch
```http
DELETE /api/matches/{matchId}
Authorization: Bearer {token}

Response: 204 No Content
```

## Real-time Events

### SignalR Hub Events

#### Client -> Server
- `JoinMatch(matchId)` - Join match room
- `SendMessage(request)` - Send message
- `Typing(request)` - Send typing indicator
- `MarkRead(request)` - Mark messages as read

#### Server -> Client
- `MessageReceived` - New message received
- `Typing` - User typing status
- `ReadReceipt` - Messages marked as read

## Database Schema

### Swipes Table
```sql
Swipes
├── Id (PK, Guid)
├── SwiperId (FK to Users)
├── TargetId (FK to Users)
├── Direction (Enum: Like/Pass)
└── CreatedAt (DateTime)
```

### Matches Table
```sql
Matches
├── Id (PK, Guid)
├── UserAId (FK to Users)
├── UserBId (FK to Users)
├── CreatedAt (DateTime)
└── Messages (Navigation)
```

### Messages Table
```sql
Messages
├── Id (PK, Guid)
├── MatchId (FK to Matches)
├── SenderId (FK to Users)
├── Content (string, nullable)
├── ImagePhotoId (FK to Photos, nullable)
├── SentAt (DateTime)
├── DeliveredAt (DateTime, nullable)
└── ReadAt (DateTime, nullable)
```

## Testing

### Unit Tests
Located in `/backend/App.Tests/Services/MatchingSystemTests.cs`
- Tests swipe processing logic
- Tests match creation
- Tests message sending/receiving
- Tests unmatch functionality
- Tests permission validation

### Integration Tests
Located in `/backend/App.Tests/Controllers/MatchingIntegrationTests.cs`
- End-to-end swipe and match flow
- Message flow between matched users
- Unmatch flow
- Authorization and permission tests
- Multiple match scenarios

### Running Tests
```bash
cd backend
dotnet test
```

## Security Considerations

1. **Authentication Required**: All endpoints require JWT authentication
2. **Authorization Checks**: Users can only:
   - View their own matches
   - Send messages in matches they're part of
   - Unmatch from their own matches
3. **Input Validation**: All inputs validated before processing
4. **Idempotency**: Duplicate swipes handled gracefully
5. **Data Isolation**: Users cannot access other users' private data

## Performance Optimizations

1. **Eager Loading**: Related data loaded efficiently to minimize queries
2. **Pagination**: Match and message lists support pagination
3. **Caching**: Feed candidates can be cached
4. **Async Operations**: All database operations are async
5. **SignalR Groups**: Efficient message broadcasting to match participants

## Future Enhancements

1. **Super Likes**: Special swipe type with immediate notification
2. **Boost**: Temporary profile visibility increase
3. **Read Receipts**: Optional read receipt settings
4. **Media Messages**: Support for images/videos in chat
5. **Video Chat**: WebRTC integration for video calls
6. **Report/Block**: User safety features
7. **Match Expiry**: Auto-unmatch after inactivity
8. **Smart Suggestions**: ML-based profile recommendations
9. **Location-based Matching**: Geo-filtering for candidates
10. **Match Statistics**: Analytics for users

## Troubleshooting

### Common Issues

1. **Matches not appearing**
   - Verify both users have liked each other
   - Check database for Match record
   - Ensure proper authorization headers

2. **Messages not sending**
   - Verify users are matched
   - Check SignalR connection status
   - Validate message content

3. **Real-time updates not working**
   - Check SignalR connection
   - Verify WebSocket support
   - Check network connectivity

4. **Swipes not processing**
   - Check for existing swipe records
   - Verify target user exists
   - Validate swipe direction

## Development Setup

### Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend Setup
```bash
cd frontend
npm install
npm start
```

### Environment Variables
Backend (`appsettings.json`):
- Database connection string
- JWT secret key
- CORS origins
- SignalR configuration

Frontend:
- API base URL
- SignalR hub URL

## Contact & Support

For questions or issues related to the matching system:
1. Check this documentation
2. Review test cases for examples
3. Check error logs for detailed messages
4. Create an issue in the project repository

---

Last Updated: 2025-01-18
Version: 1.0.0