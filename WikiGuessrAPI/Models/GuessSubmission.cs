namespace WikiGuessrAPI.Models;

public class GuessSubmission
{
    public Guid PlayerId { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public TimeSpan TimeTaken { get; init; }
}
