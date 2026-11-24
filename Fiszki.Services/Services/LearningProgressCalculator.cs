namespace Fiszki.Services.Services;

/// <summary>The one rule for the learning progress percentage, shared by the Flashcards page and the leaderboard.</summary>
public static class LearningProgressCalculator
{
    /// <summary>
    /// Share of archived flashcards among all flashcards, in whole percent rounded down, so that 100 is
    /// reached only when every flashcard is archived. 0 when there are no flashcards.
    /// </summary>
    public static int Percent(int archived, int total)
    {
        if (total <= 0 || archived <= 0)
        {
            return 0;
        }

        return (int)Math.Min(100L, archived * 100L / total);
    }
}
