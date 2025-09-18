# Tinder Clone API Tests

This test suite provides comprehensive integration testing for the Tinder Clone backend API.

## Test Structure

The test suite is organized as follows:

- **App.Tests.csproj** - Test project configuration with all necessary dependencies
- **IntegrationTestBase.cs** - Base class for all integration tests with helper methods
- **Fixtures/TestWebApplicationFactory.cs** - Test server configuration with PostgreSQL container
- **Controllers/** - Test files for each API controller

## Test Coverage

### Authentication Tests (`AuthControllerTests.cs`)
- User registration with validation
- Login with valid/invalid credentials
- Token refresh functionality
- Logout functionality
- Duplicate email handling
- Password validation

### User Profile Tests (`UserControllerTests.cs`)
- Get current user profile
- Update profile information
- Update location
- Get public profiles
- Photo upload/delete
- Authorization checks

### Swipes & Matches Tests (`SwipesAndMatchesControllerTests.cs`)
- Swipe processing (like/pass)
- Mutual match detection
- Get matches list with pagination
- Get match details
- Send messages in matches
- Get message history
- Self-swipe prevention

### Feed Tests (`FeedControllerTests.cs`)
- Get candidate profiles
- Radius-based filtering
- Pagination support
- Exclude already swiped users
- Authorization requirements

## Prerequisites

- .NET 8 SDK
- Docker (for PostgreSQL test container)
- At least 2GB of available RAM

## Running Tests

### Using the test script:
```bash
cd backend
./test.sh
```

### Using dotnet CLI:
```bash
cd backend/App.Tests
dotnet test
```

### Running specific test classes:
```bash
dotnet test --filter "FullyQualifiedName~AuthControllerTests"
```

### Running with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Configuration

The tests use:
- **Testcontainers** for PostgreSQL database isolation
- **Respawn** for database cleanup between tests
- **xUnit** as the test framework
- **FluentAssertions** for readable assertions
- **Microsoft.AspNetCore.Mvc.Testing** for integration testing

## Database Management

Each test class:
1. Starts a fresh PostgreSQL container
2. Runs EF Core migrations
3. Resets the database before each test
4. Disposes the container after all tests complete

This ensures complete test isolation and reproducibility.

## Writing New Tests

To add new tests:

1. Create a test class inheriting from `IntegrationTestBase`
2. Implement `IAsyncLifetime` for setup/teardown
3. Use the helper methods for authentication and API calls
4. Follow the existing naming conventions

Example:
```csharp
public class NewControllerTests : IntegrationTestBase, IAsyncLifetime
{
    public NewControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    public async Task InitializeAsync() => await ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task YourTest_WithCondition_ShouldExpectedResult()
    {
        // Arrange
        var token = await AuthenticateAsync("test@example.com");

        // Act
        var response = await Client.GetAsync("/api/endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Troubleshooting

### Docker not running
Ensure Docker Desktop is running before executing tests.

### Port conflicts
The test container uses random ports to avoid conflicts.

### Slow test execution
First test run downloads the PostgreSQL image. Subsequent runs are faster.

### Database migration failures
Check that all migrations in the main project compile and run successfully.

## CI/CD Integration

These tests are designed to run in CI/CD pipelines. Ensure your pipeline:
1. Has Docker support enabled
2. Has sufficient memory allocated
3. Runs tests sequentially if resources are limited