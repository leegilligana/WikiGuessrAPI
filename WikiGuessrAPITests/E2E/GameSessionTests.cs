using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.E2E;

public class GameSessionTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateSessionTest()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsync("/api/GameSession?numRounds=5&hostPlayerName=Aidan", null, TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var data = JsonSerializer.Deserialize<SessionCreatedResponse>(content);
        data.Should().NotBeNull();

        Guid sessionId = Guid.Parse(data.SessionId);
        Guid playerId = Guid.Parse(data.PlayerId);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        sessionId.Should().NotBeEmpty();
        playerId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FetchSessionTest(bool shouldFindSession)
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageCachedSessionInfo>();
        var sessionToFetch = new Session()
        {
            PlayerNames = new()
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new()
            {
                { Guid.NewGuid(), 0 },
            },
            Id = shouldFindSession ? sessionGuid : Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            Round = 0,
            RoundLimit = 10,
        };
        await sessionCache.AddSessionToCacheAsync(sessionToFetch);

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/GameSession/{sessionGuid}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(shouldFindSession ? HttpStatusCode.OK : HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSessionTest()
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageCachedSessionInfo>();
        var sessionToDelete = new Session()
        {
            PlayerNames = new()
            {
                { Guid.NewGuid(), "Scobert Dobert" },
            },
            PlayerScores = new()
            {
                { Guid.NewGuid(), 0 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            Round = 0,
            RoundLimit = 10,
        };

        await sessionCache.AddSessionToCacheAsync(sessionToDelete);
        var client = factory.CreateClient();
        var response = await client.DeleteAsync($"/api/GameSession/{sessionGuid}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchResponse = await client.GetAsync($"/api/GameSession/{sessionGuid}", TestContext.Current.CancellationToken);
        fetchResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public record SessionCreatedResponse(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("playerId")] string PlayerId
);
