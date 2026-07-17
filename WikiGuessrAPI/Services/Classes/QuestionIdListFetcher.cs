namespace WikiGuessrAPI.Services.Classes;

public static class QuestionIdListFetcher
{
    public static IEnumerable<int> GuidToQuestionIdList(Guid seedGuid, int upperBound, int maxQuestions = 20)
    {
        if (upperBound < 0)
        {
            return [];
        }

        var seed = GuidToInt(seedGuid);
        var random = new Random(seed);
        return [.. Enumerable.Range(0, maxQuestions).Select(_ => random.Next(upperBound))];
    }

    public static int GuidToInt(Guid seedGuid)
    {
        ReadOnlySpan<byte> bytes = seedGuid.ToByteArray();
        ReadOnlySpan<int> ints = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, int>(bytes);
        return ints[0] ^ ints[1] ^ ints[2] ^ ints[3];
    }
}
