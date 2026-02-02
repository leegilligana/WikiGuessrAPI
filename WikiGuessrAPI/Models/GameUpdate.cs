using WikiGuessrAPI.Models.Enums;

namespace WikiGuessrAPI.Models;

internal class GameUpdate
{
    public required Dictionary<string, int> Leaderboard { get; init; }

    public string? NextQuestionName { get; init; }

    public QuestionType? NextQuestionType { get; init; }
}
