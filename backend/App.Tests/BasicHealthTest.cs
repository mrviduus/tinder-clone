using System.Net;
using App.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace App.Tests;

public class BasicHealthTest : IntegrationTestBase, IAsyncLifetime
{
    public BasicHealthTest(TestWebApplicationFactory factory) : base(factory)
    {
    }

    public async Task InitializeAsync()
    {
        // Don't reset database for health check
        await Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task HealthCheck_ShouldReturnOK()
    {
        var response = await Client.GetAsync("/healthz");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetInfo_ShouldReturnSomething()
    {
        var response = await Client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        System.Console.WriteLine($"Root endpoint returned: {response.StatusCode} - {content}");

        // Just verify we get some response
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}