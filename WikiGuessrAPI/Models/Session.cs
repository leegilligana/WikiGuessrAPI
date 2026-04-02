namespace WikiGuessrAPI.Models;

public class Session
{
    public Guid Id { get; init; }

    public Guid Seed { get; init; }

    public required Dictionary<Guid, (string Name, int Score)> PlayerScores { get; init; }

    public int Round { get; init; }

    public int RoundLimit { get; init; }
}
