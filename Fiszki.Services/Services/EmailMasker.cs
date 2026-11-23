namespace Fiszki.Services.Services;

/// <summary>Masks email addresses so the leaderboard never exposes a full address.</summary>
internal static class EmailMasker
{
    private const string MaskText = "***";
    private const int MaxVisibleChars = 2;

    /// <summary>
    /// Keeps up to the first two characters of the local part, replaces the rest with "***" and keeps
    /// the domain: "jan.kowalski@example.com" becomes "ja***@example.com". The local part is never
    /// shown in full, so one- and two-character local parts keep fewer characters.
    /// </summary>
    public static string Mask(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return MaskText;
        }

        var at = email.LastIndexOf('@');
        var local = at >= 0 ? email[..at] : email;
        var domain = at >= 0 ? email[at..] : string.Empty;

        var visible = Math.Min(MaxVisibleChars, Math.Max(local.Length - 1, 0));
        return local[..visible] + MaskText + domain;
    }
}
