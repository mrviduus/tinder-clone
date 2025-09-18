using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using App.DTOs;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;
using App.Domain;

namespace App.Tests.Controllers;

public class UserControllerTests : IntegrationTestBase, IAsyncLifetime
{
    public UserControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetProfile_WithValidAuth_ShouldReturnUserProfile()
    {
        var token = await CreateAndAuthenticateUser("profile@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("profile@test.com");
    }

    [Fact]
    public async Task GetProfile_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldUpdateSuccessfully()
    {
        var token = await CreateAndAuthenticateUser("update@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateProfileRequest
        {
            Bio = "Updated bio text",
            DisplayName = "Updated Name"
        };

        var response = await Client.PutAsJsonAsync("/api/me", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.Bio.Should().Be("Updated bio text");
        profile.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateLocation_WithValidCoordinates_ShouldUpdateSuccessfully()
    {
        var token = await CreateAndAuthenticateUser("location@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var locationRequest = new UpdateLocationRequest
        {
            Lat = 40.7128,
            Lng = -74.0060
        };

        var response = await Client.PutAsJsonAsync("/api/me/location", locationRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetPublicProfile_WithValidId_ShouldReturnProfile()
    {
        var user1Token = await CreateAndAuthenticateUser("user1@test.com");
        var user2Response = await RegisterUserAsync("user2@test.com", "TestPassword123!", "User Two");

        var registerResponse = await user2Response.Content.ReadFromJsonAsync<RegisterResponse>();
        var user2Id = registerResponse!.UserId;

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        var response = await Client.GetAsync($"/api/users/{user2Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PublicProfileDto>();
        profile.Should().NotBeNull();
        profile!.DisplayName.Should().Be("User Two");
    }

    [Fact]
    public async Task GetPublicProfile_WithInvalidId_ShouldReturnNotFound()
    {
        var token = await CreateAndAuthenticateUser("test@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadPhoto_WithValidFile_ShouldUploadSuccessfully()
    {
        var token = await CreateAndAuthenticateUser("photo@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var imageBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(imageContent, "file", "test.png");

        var response = await Client.PostAsync("/api/me/photos", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var photoResponse = await response.Content.ReadFromJsonAsync<PhotoUploadResponse>();
        photoResponse.Should().NotBeNull();
        photoResponse!.PhotoId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeletePhoto_WithValidPhotoId_ShouldDeleteSuccessfully()
    {
        var token = await CreateAndAuthenticateUser("deletephoto@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var imageBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(imageContent, "file", "test.png");

        var uploadResponse = await Client.PostAsync("/api/me/photos", content);
        var photoResponse = await uploadResponse.Content.ReadFromJsonAsync<PhotoUploadResponse>();

        var deleteResponse = await Client.DeleteAsync($"/api/me/photos/{photoResponse!.PhotoId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeletePhoto_WithInvalidPhotoId_ShouldReturnNotFound()
    {
        var token = await CreateAndAuthenticateUser("notfound@test.com");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.DeleteAsync($"/api/me/photos/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> CreateAndAuthenticateUser(string email)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "TestPassword123!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "TestPassword123!"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    private class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }

    private class ProfileDto
    {
        public string Email { get; set; } = default!;
        public string? Bio { get; set; }
        public string? DisplayName { get; set; }
    }

    private class PublicProfileDto
    {
        public string DisplayName { get; set; } = default!;
    }

    private class PhotoUploadResponse
    {
        public Guid PhotoId { get; set; }
    }
}