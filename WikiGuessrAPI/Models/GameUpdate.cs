using WikiGuessrAPI.Models.Enums;

namespace WikiGuessrAPI.Models;

public class GameUpdate
{
    public required Dictionary<string, int> Leaderboard { get; init; }

    public string? NextQuestionName { get; init; }

    public HintType? NextQuestionType { get; init; }
}
