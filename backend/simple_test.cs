using System.Net.Http.Json;
using System.Net;
using App.DTOs;
using App.Domain;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:8080");

        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!",
            DisplayName = "Test User",
            BirthDate = DateTime.UtcNow.AddYears(-25),
            Gender = Gender.Male
        };

        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            Console.WriteLine($"Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}