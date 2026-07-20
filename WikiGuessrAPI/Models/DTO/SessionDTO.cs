namespace WikiGuessrAPI.Models.DTO;

public class SessionDTO(Guid id, int round, int roundLimit, Dictionary<string, int> playerScores)
{
    public Guid Id { get; init; } = id;

    public int Round { get; init; } = round;

    public int RoundLimit { get; init; } = roundLimit;

    public Dictionary<string, int> PlayerScores { get; init; } = playerScores;

    public static SessionDTO FromSession(Session session)
    {
        return new SessionDTO(
            session.Id,
            session.Round,
            session.RoundLimit,
            session.PlayerNames.ToDictionary(
                kvp => kvp.Value,
                kvp => session.PlayerScores.GetValueOrDefault(kvp.Key, 0)));
    }
}
