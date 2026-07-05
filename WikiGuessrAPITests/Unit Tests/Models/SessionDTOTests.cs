using WikiGuessrAPI.Models;
using WikiGuessrAPI.Models.DTO;
using Xunit;

namespace WikiGuessrAPITests.Unit_Tests.Models;

public class SessionDTOTests
{
    [Fact]
    public void SessionDTOMappingTest()
    {
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var originalSession = new Session
        {
            Id = Guid.NewGuid(),
            Seed = Guid.NewGuid(),
            Round = 3,
            RoundLimit = 5,
            PlayerNames = new Dictionary<Guid, string>
            {
                { player1Id, "Alice" },
                { player2Id, "Bob" },
            },
            PlayerScores = new Dictionary<Guid, int>
            {
                { player1Id, 1500 },
                { player2Id, 1200 },
            },
        };

        var dto = SessionDTO.FromSession(originalSession);

        Assert.Equal(originalSession.Id, dto.Id);
        Assert.Equal(originalSession.Round, dto.Round);
        Assert.Equal(originalSession.RoundLimit, dto.RoundLimit);

        Assert.NotNull(dto.PlayerScores);
        Assert.Equal(2, dto.PlayerScores.Count);

        Assert.True(dto.PlayerScores.ContainsKey("Alice"));
        Assert.Equal(1500, dto.PlayerScores["Alice"]);

        Assert.True(dto.PlayerScores.ContainsKey("Bob"));
        Assert.Equal(1200, dto.PlayerScores["Bob"]);
    }
}
