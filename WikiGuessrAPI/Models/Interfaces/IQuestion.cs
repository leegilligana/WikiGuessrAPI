namespace WikiGuessrAPI.Models.Interfaces
{
    public interface IQuestion
    {
        public string Name { get; init; }

        public int Id { get; init; }

        public int AnswerId { get; init; }

        public float Latitude { get; init; }

        public float Longitude { get; init; }

        public string? Url { get; init; }

        public int? Difficulty { get; init; }
    }
}
