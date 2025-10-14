using System.Text.RegularExpressions;
namespace Fiszki.Components.Pages.Generate;


public static class GenerationValidation
{
    public const int MinSourceLength = 50;
    public const int MaxSourceLength = 5000;
    public const int DefaultMaxCards = 20;
    public const int MaxAllowedCards = 50;
    
    private static readonly Regex LanguageCodeRegex = new("^[a-z]{2}(-[A-Z]{2})?$");
    
    public static bool IsValidSourceText(string text)
        => !string.IsNullOrWhiteSpace(text) && 
           text.Length >= MinSourceLength && 
           text.Length <= MaxSourceLength;
           
    public static bool IsValidLanguageCode(string code)
        => !string.IsNullOrWhiteSpace(code) && 
           LanguageCodeRegex.IsMatch(code);
           
    public static bool IsValidMaxCards(int count)
        => count > 0 && count <= MaxAllowedCards;
        
    public static string GetSourceTextError(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Source text is required";
            
        if (text.Length < MinSourceLength)
            return $"Source text must be at least {MinSourceLength} characters";
            
        if (text.Length > MaxSourceLength)
            return $"Source text must not exceed {MaxSourceLength} characters";
            
        return string.Empty;
    }
    
    public static string GetLanguageCodeError(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Language code is required";
            
        if (!LanguageCodeRegex.IsMatch(code))
            return "Invalid language code format (e.g. 'en' or 'en-US')";
            
        return string.Empty;
    }
    
    public static string GetMaxCardsError(int count)
    {
        if (count <= 0)
            return "Number of cards must be positive";
            
        if (count > MaxAllowedCards)
            return $"Number of cards must not exceed {MaxAllowedCards}";
            
        return string.Empty;
    }
}
