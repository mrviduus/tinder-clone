using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using App.Data;
using Xunit;

namespace App.Tests.Fixtures;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=tinder_test;Username=appuser;Password=appsecret",
                ["JwtSettings:SecretKey"] = "TestSecretKey123456789012345678901234567890",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "15",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Clear all data from tables
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE messages CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE matches CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE swipes CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE photos CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE profiles CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE blocks CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE refresh_tokens CASCADE");
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users CASCADE");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}