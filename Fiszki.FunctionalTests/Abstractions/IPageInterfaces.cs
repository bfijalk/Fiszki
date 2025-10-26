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
}
