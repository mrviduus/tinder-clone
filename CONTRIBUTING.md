# Contributing to Tinder Clone

First off, thank you for considering contributing to this project! It's people like you that make this Tinder Clone such a great learning resource and portfolio example.

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible using the issue template.

**Great Bug Reports** tend to have:
- A quick summary and/or background
- Steps to reproduce
- What you expected would happen
- What actually happens
- Notes (possibly including why you think this might be happening)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:
- A clear and descriptive title
- A detailed description of the proposed enhancement
- Explain why this enhancement would be useful
- List any similar features in other applications

### Pull Requests

1. Fork the repo and create your branch from `main`.
2. If you've added code that should be tested, add tests.
3. If you've changed APIs, update the documentation.
4. Ensure the test suite passes.
5. Make sure your code follows the existing code style.
6. Issue that pull request!

## Development Setup

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Docker and Docker Compose
- PostgreSQL 16 (or use Docker)

### Local Development

1. **Clone your fork**
   ```bash
   git clone https://github.com/your-username/tinder-clone.git
   cd tinder-clone
   ```

2. **Backend Setup**
   ```bash
   cd backend/App
   dotnet restore
   dotnet build
   dotnet test ../App.Tests
   ```

3. **Frontend Setup**
   ```bash
   cd frontend
   npm install
   npm run type-check
   ```

4. **Database Setup**
   ```bash
   docker-compose up -d db
   cd backend/App
   dotnet ef database update
   ```

### Code Style Guidelines

#### C# / .NET Backend
- Use PascalCase for public members and types
- Use camelCase for private members with underscore prefix (_camelCase)
- Use async/await properly
- Add XML documentation comments for public APIs
- Follow Microsoft's C# coding conventions

Example:
```csharp
public class UserService : IUserService
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        // Implementation
    }
}
```

#### TypeScript / React Native Frontend
- Use TypeScript for all new code
- Use functional components with hooks
- Use PascalCase for components
- Use camelCase for functions and variables
- Define proper TypeScript types/interfaces
- Avoid `any` type usage

Example:
```typescript
interface UserProfileProps {
  userId: string;
  onUpdate: (profile: UserProfile) => void;
}

export const UserProfile: React.FC<UserProfileProps> = ({ userId, onUpdate }) => {
  // Component implementation
};
```

### Testing

#### Backend Testing
```bash
cd backend/App.Tests
dotnet test --logger "console;verbosity=detailed"
```

#### Frontend Testing
```bash
cd frontend
npm run type-check
npm run lint
```

### Commit Messages

We follow the Conventional Commits specification:

- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation only changes
- `style:` Changes that don't affect code meaning
- `refactor:` Code change that neither fixes a bug nor adds a feature
- `perf:` Performance improvement
- `test:` Adding missing tests
- `chore:` Changes to build process or auxiliary tools

Examples:
- `feat: add super like functionality to swipe system`
- `fix: resolve SignalR reconnection issue on iOS`
- `docs: update API documentation for match endpoints`

### Database Migrations

When making database changes:

1. Create a new migration:
   ```bash
   cd backend/App
   dotnet ef migrations add YourMigrationName
   ```

2. Test the migration:
   ```bash
   dotnet ef database update
   ```

3. Include both the migration and any necessary seed data

### API Guidelines

- Use RESTful conventions
- Return appropriate HTTP status codes
- Include proper error responses
- Add Swagger documentation
- Version APIs when making breaking changes

### Pull Request Process

1. Update the README.md with details of changes to the interface
2. Update documentation for any changed functionality
3. The PR will be merged once you have approval from a maintainer

## Project Structure

```
tinder-clone/
├── backend/             # ASP.NET Core backend
│   ├── App/            # Main application
│   └── App.Tests/      # Unit tests
├── frontend/           # React Native frontend
├── docs/              # Documentation
├── scripts/           # Utility scripts
└── .github/           # GitHub specific files
```

## Need Help?

Feel free to open an issue for:
- Questions about the codebase
- Clarification on requirements
- Help with setup issues

## Recognition

Contributors will be recognized in the README.md file. Thank you for your contributions!