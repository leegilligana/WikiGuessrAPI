namespace WikiGuessrAPI.Models;

internal class GuessSubmission
{
    public int PlayerId { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public TimeSpan TimeTaken { get; init; }
}
