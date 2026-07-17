namespace WikiGuessrAPI.Models;

public class Article
{
    public required string Title { get; init; }

    public required Uri Uri { get; init; }

    public Uri? ImageUri { get; init; }
}
