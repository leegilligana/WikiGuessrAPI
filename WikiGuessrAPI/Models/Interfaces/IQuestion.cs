namespace WikiGuessrAPI.Models.Interfaces;

internal interface IQuestion
{
    public string Name { get; init; }

    public int Id { get; init; }

    public int AnswerId { get; init; }

    public int ArticleId { get; init; }

    public float Latitude { get; init; }

    public float Longitude { get; init; }

    public Enums.QuestionType Type { get; init; }

    public int? Difficulty { get; init; }
}
