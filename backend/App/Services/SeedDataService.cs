using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using App.Data;
using App.Domain;

namespace App.Services;

public class SeedDataService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public SeedDataService(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (_context.Users.Any())
        {
            return; // Database already seeded
        }

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

        // Create admin user first
        var adminUser = new User
        {
            UserName = "admin@tinder.com",
            Email = "admin@tinder.com",
            EmailConfirmed = true
        };

        var adminResult = await _userManager.CreateAsync(adminUser, "Admin123!");
        if (adminResult.Succeeded)
        {
            // Create admin profile
            var adminLocation = geometryFactory.CreatePoint(new Coordinate(-74.0060, 40.7128)); // NYC
            var adminProfile = new Profile
            {
                UserId = adminUser.Id,
                DisplayName = "Admin",
                BirthDate = DateTime.UtcNow.AddYears(-30),
                Gender = Gender.NonBinary,
                Bio = "Admin account for testing and management",
                SearchGender = Gender.Female,
                AgeMin = 18,
                AgeMax = 50,
                MaxDistanceKm = 100,
                Location = adminLocation,
                LocationUpdatedAt = DateTime.UtcNow
            };

            _context.Profiles.Add(adminProfile);
        }

        // Create test users
        var users = new[]
        {
            new { Email = "alice@example.com", Name = "Alice", Age = 25, Gender = Gender.Female, Lat = 40.7128, Lng = -74.0060 }, // NYC
            new { Email = "bob@example.com", Name = "Bob", Age = 28, Gender = Gender.Male, Lat = 40.7589, Lng = -73.9851 }, // NYC
            new { Email = "charlie@example.com", Name = "Charlie", Age = 30, Gender = Gender.Male, Lat = 40.7831, Lng = -73.9712 }, // NYC
            new { Email = "diana@example.com", Name = "Diana", Age = 26, Gender = Gender.Female, Lat = 40.7505, Lng = -73.9934 }, // NYC
            new { Email = "eve@example.com", Name = "Eve", Age = 24, Gender = Gender.Female, Lat = 40.7282, Lng = -73.7949 }, // NYC
        };

        var createdUsers = new List<User>();

        foreach (var userData in users)
        {
            var user = new User
            {
                UserName = userData.Email,
                Email = userData.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                // Create profile
                var birthDate = DateTime.UtcNow.AddYears(-userData.Age);
                var location = geometryFactory.CreatePoint(new Coordinate(userData.Lng, userData.Lat));

                var profile = new Profile
                {
                    UserId = user.Id,
                    DisplayName = userData.Name,
                    BirthDate = birthDate,
                    Gender = userData.Gender,
                    Bio = $"Hi, I'm {userData.Name}! Looking to meet new people in the city.",
                    SearchGender = userData.Gender == Gender.Male ? Gender.Female : Gender.Male,
                    AgeMin = 22,
                    AgeMax = 35,
                    MaxDistanceKm = 25,
                    Location = location,
                    LocationUpdatedAt = DateTime.UtcNow
                };

                _context.Profiles.Add(profile);
                createdUsers.Add(user);
            }
        }

        await _context.SaveChangesAsync();

        // Add sample photos for users
        if (createdUsers.Count >= 2)
        {
            var alice = createdUsers[0];
            var bob = createdUsers[1];

            // Create sample photos (small placeholder images)
            var alicePhoto1 = new Photo
            {
                UserId = alice.Id,
                ContentType = "image/jpeg",
                SizeBytes = 1024,
                Data = CreateSampleImageData("Alice", "lightblue"),
                IsPrimary = true
            };

            var alicePhoto2 = new Photo
            {
                UserId = alice.Id,
                ContentType = "image/jpeg",
                SizeBytes = 1024,
                Data = CreateSampleImageData("Alice 2", "pink"),
                IsPrimary = false
            };

            var bobPhoto1 = new Photo
            {
                UserId = bob.Id,
                ContentType = "image/jpeg",
                SizeBytes = 1024,
                Data = CreateSampleImageData("Bob", "lightgreen"),
                IsPrimary = true
            };

            var bobPhoto2 = new Photo
            {
                UserId = bob.Id,
                ContentType = "image/jpeg",
                SizeBytes = 1024,
                Data = CreateSampleImageData("Bob 2", "orange"),
                IsPrimary = false
            };

            _context.Photos.AddRange(alicePhoto1, alicePhoto2, bobPhoto1, bobPhoto2);
            await _context.SaveChangesAsync();
        }

        // Create some test swipes and matches
        if (createdUsers.Count >= 4)
        {
            var alice = createdUsers[0];
            var bob = createdUsers[1];
            var charlie = createdUsers[2];
            var diana = createdUsers[3];

            // Alice and Bob mutual like (should create match)
            _context.Swipes.AddRange(
                new Swipe { SwiperId = alice.Id, TargetId = bob.Id, Direction = SwipeDirection.Like },
                new Swipe { SwiperId = bob.Id, TargetId = alice.Id, Direction = SwipeDirection.Like }
            );

            // Create the match
            var match = new Match
            {
                UserAId = alice.Id < bob.Id ? alice.Id : bob.Id,
                UserBId = alice.Id < bob.Id ? bob.Id : alice.Id
            };
            _context.Matches.Add(match);

            // Alice and Charlie - Alice likes, Charlie hasn't swiped yet
            _context.Swipes.Add(new Swipe { SwiperId = alice.Id, TargetId = charlie.Id, Direction = SwipeDirection.Like });

            // Diana passes on Bob
            _context.Swipes.Add(new Swipe { SwiperId = diana.Id, TargetId = bob.Id, Direction = SwipeDirection.Pass });

            await _context.SaveChangesAsync();

            // Add some test messages to the Alice-Bob match
            var messages = new[]
            {
                new Message { MatchId = match.Id, SenderId = alice.Id, Content = "Hey Bob! 👋", SentAt = DateTime.UtcNow.AddHours(-2), DeliveredAt = DateTime.UtcNow.AddHours(-2) },
                new Message { MatchId = match.Id, SenderId = bob.Id, Content = "Hi Alice! How are you?", SentAt = DateTime.UtcNow.AddHours(-1), DeliveredAt = DateTime.UtcNow.AddHours(-1) },
                new Message { MatchId = match.Id, SenderId = alice.Id, Content = "I'm great! Love your profile 😊", SentAt = DateTime.UtcNow.AddMinutes(-30), DeliveredAt = DateTime.UtcNow.AddMinutes(-30) }
            };

            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();
        }
    }

    private static byte[] CreateSampleImageData(string text, string color)
    {
        // Create a minimal JPEG header and data
        // This is a very basic placeholder - in a real app you'd use actual image processing
        var data = new byte[1024];
        var textBytes = System.Text.Encoding.UTF8.GetBytes($"{text}-{color}");
        Array.Copy(textBytes, 0, data, 0, Math.Min(textBytes.Length, data.Length));
        return data;
    }
}