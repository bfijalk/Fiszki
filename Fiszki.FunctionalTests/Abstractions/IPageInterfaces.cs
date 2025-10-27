using Microsoft.Playwright;

namespace Fiszki.FunctionalTests.Abstractions;

public interface INavigable
{
    Task NavigateAsync();
}

public interface IFormPage
{
    Task EnterEmailAsync(string email);
}

public interface ILoginPage : INavigable, IFormPage
{
    Task EnterPasswordAsync(string password);
    Task ClickLoginAsync();
    Task<bool> IsLoginButtonDisabledAsync();
}

public interface IRegisterPage : INavigable, IFormPage
{
    Task EnterPasswordAsync(string password);
    Task EnterConfirmPasswordAsync(string confirmPassword);
    ILocator PasswordMismatchMessage { get; }
}

public interface IFlashcardGenerationPage : INavigable
{
    Task EnterSourceTextAsync(string text);
    Task SetMaximumCardsAsync(int maxCards);
    Task ClickGenerateFlashcardsAsync();
    Task ClickAcceptAllAsync();
    Task ClickSaveSelectedAsync();
}

public interface IFlashcardsPage : INavigable
{
    Task<bool> IsFlashcardVisibleAsync(string flashcardText);
    Task ClickFlashcardAsync(string flashcardText);
    Task<bool> IsLoadingAsync();
    Task<bool> HasFlashcardsAsync();
    Task<int> GetTotalFlashcardCountAsync();
    Task<int> GetAiGeneratedCountAsync();
    Task<int> GetManualCountAsync();
    Task ClickFilterAsync(string filterType); // "Total", "Ai", "Manual"
    Task ClearFilterAsync();
    Task ToggleViewModeAsync();
    Task<bool> IsCardViewActiveAsync();
    Task ClickGenerateMoreCardsAsync();
    Task ClickAddManualCardAsync();
    Task FlipCardAsync(string cardText);
    Task<bool> IsCardFlippedAsync(string cardText);
    Task EditCardAsync(string cardText);
    Task DeleteCardAsync(string cardText);
    Task ConfirmDeleteAsync();
    Task CancelDeleteAsync();
    
    // Manual card creation methods
    Task EnterQuestionAsync(string question);
    Task EnterAnswerAsync(string answer);
    Task EnterTagsAsync(string tags);
    Task ClickCreateCardAsync();
    Task CancelCreateCardAsync();
    Task<bool> IsCreateModalVisibleAsync();
    Task<bool> IsDeleteModalVisibleAsync();
    Task<string?> GetCreateErrorAsync();
}
