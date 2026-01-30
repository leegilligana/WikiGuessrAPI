namespace WikiGuessrAPI.Models.Interfaces;

internal interface IGuess
{
    public float Latitude { get; init; }

    public float Longitude { get; init; }

    public int PlayerId { get; init; }
}
