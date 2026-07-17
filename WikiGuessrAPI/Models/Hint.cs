namespace WikiGuessrAPI.Models;

public class Hint
{
    public required string Name { get; init; }

    public int Id { get; init; }

    public int AnswerId { get; init; }

    public int ArticleId { get; init; }

    public Enums.HintType Type { get; init; }

    public int? Difficulty { get; init; }
}
