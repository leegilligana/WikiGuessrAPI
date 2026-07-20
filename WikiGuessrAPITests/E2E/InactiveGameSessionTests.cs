using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.DTO;
using WikiGuessrAPI.Services.Classes;
using WikiGuessrAPI.Services.Interfaces;
using Xunit;

namespace WikiGuessrAPITests.E2E;

public class InactiveGameSessionTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IHostedService) &&
                         d.ImplementationType == typeof(GameSessionOrchestrator));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
            });
        });

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
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var playerGuid = Guid.NewGuid();
        var sessionToFetch = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid, "Player1" },
            },
            PlayerScores = new()
            {
                { playerGuid, 10 },
            },
            Id = shouldFindSession ? sessionGuid : Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            HostId = playerGuid,
            Round = 0,
            RoundLimit = 10,
        };

        await sessionCache.AddSessionToCacheAsync(sessionToFetch);

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/GameSession/{sessionGuid}", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(shouldFindSession ? HttpStatusCode.OK : HttpStatusCode.NotFound);

        if (shouldFindSession)
        {
            var dto = await response.Content.ReadFromJsonAsync<SessionDTO>(cancellationToken: TestContext.Current.CancellationToken);

            dto.Should().NotBeNull();

            dto.Id.Should().Be(sessionToFetch.Id);
            dto.Round.Should().Be(sessionToFetch.Round);
            dto.RoundLimit.Should().Be(sessionToFetch.RoundLimit);

            dto.PlayerScores.Should().NotBeNull();
            dto.PlayerScores.Should().HaveCount(1);
            dto.PlayerScores.Should().ContainKey("Player1");
            dto.PlayerScores["Player1"].Should().Be(10);
        }
    }

    [Fact]
    public async Task DeleteSessionTest()
    {
        var sessionGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
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
            HostId = Guid.NewGuid(),
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

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task RemovePlayerAsHostTest_SessionFound(bool isHost, bool shouldFindPlayer)
    {
        var sessionGuid = Guid.NewGuid();
        var playerGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var sessionToModify = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid, "Player1" },
            },
            PlayerScores = new()
            {
                { playerGuid, 10 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            HostId = playerGuid,
            Round = 0,
            RoundLimit = 10,
        };

        await sessionCache.AddSessionToCacheAsync(sessionToModify);

        var client = factory.CreateClient();
        var queryGuid = isHost ? playerGuid : Guid.NewGuid();
        var playerToKick = shouldFindPlayer ? "Player1" : "NonExistentPlayer";
        var response = await client.DeleteAsync($"/api/GameSession/{sessionGuid}/hosts/{queryGuid}/playerNames/{playerToKick}", TestContext.Current.CancellationToken);

        var expectedStatusCode = isHost ? (shouldFindPlayer ? HttpStatusCode.OK : HttpStatusCode.NotFound) : HttpStatusCode.Forbidden;
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task RemovePlayerFromSessionAsHost_SessionNotFound()
    {
        var sessionGuid = Guid.NewGuid();
        var playerGuid = Guid.NewGuid();
        var client = factory.CreateClient();
        var response = await client.DeleteAsync($"/api/GameSession/{sessionGuid}/hosts/{playerGuid}/playerNames/Player1", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task DeleteSessionIfHost(bool isHost, bool sessionFound)
    {
        var sessionGuid = Guid.NewGuid();
        var playerGuid = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var sessionToModify = new Session()
        {
            PlayerNames = new()
            {
                { playerGuid, "Player1" },
            },
            PlayerScores = new()
            {
                { playerGuid, 10 },
            },
            Id = sessionGuid,
            Seed = Guid.NewGuid(),
            HostId = playerGuid,
            Round = 0,
            RoundLimit = 10,
        };

        await sessionCache.AddSessionToCacheAsync(sessionToModify);

        var client = factory.CreateClient();
        var queryPlayerGuid = isHost ? playerGuid : Guid.NewGuid();
        var querySessionGuid = sessionFound ? sessionGuid : Guid.NewGuid();
        var response = await client.DeleteAsync($"/api/GameSession/{querySessionGuid}/hosts/{queryPlayerGuid}", TestContext.Current.CancellationToken);

        var expectedStatusCode = sessionFound ? (isHost ? HttpStatusCode.OK : HttpStatusCode.Forbidden) : HttpStatusCode.NotFound;
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task GetAllInactiveSessions()
    {
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var session1 = new Session()
        {
            PlayerNames = new()
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new()
            {
                { Guid.NewGuid(), 10 },
            },
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            HostId = Guid.NewGuid(),
            Round = 0,
            RoundLimit = 10,
            IsActive = true,
        };

        await sessionCache.AddSessionToCacheAsync(session1);

        var sessions = await sessionCache.GetAllInactiveSessionsAsync();

        sessions.Should().HaveCountGreaterThan(0)
            .And.NotContain(s => s.IsActive);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(0)]
    public async Task SetSessionTTL(int ttl)
    {
        using var scope = factory.Services.CreateScope();
        var sessionCache = scope.ServiceProvider.GetService<IManageInactiveSessionCache>();
        var id = Guid.NewGuid();
        var session1 = new Session()
        {
            PlayerNames = new()
            {
                { Guid.NewGuid(), "Player1" },
            },
            PlayerScores = new()
            {
                { Guid.NewGuid(), 10 },
            },
            Id = id,
            Seed = Guid.NewGuid(),
            HostId = Guid.NewGuid(),
            Round = 0,
            RoundLimit = 10,
            IsActive = true,
        };

        await sessionCache.AddSessionToCacheAsync(session1);
        await sessionCache.SetSessionTTLInSecondsAsync(id, ttl);

        var retrieval = await sessionCache.FetchSessionAsync(id);

        if (ttl <= 0)
        {
            retrieval.Should().BeNull();
        }
        else
        {
            retrieval.Should().NotBeNull();
        }
    }
}

public record SessionCreatedResponse(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("playerId")] string PlayerId
);
