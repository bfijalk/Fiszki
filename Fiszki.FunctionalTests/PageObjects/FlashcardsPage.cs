using Microsoft.Playwright;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class FlashcardsPage : BasePage, IFlashcardsPage
{
    public FlashcardsPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    // Expose the Page property for step access
    public IPage PageInstance => Page;

    public async Task NavigateAsync()
    {
        // Navigate to flashcards page by clicking the navigation link (like in the working example)
        var flashcardsLink = Page.GetByRole(AriaRole.Link, new() { Name = "Flashcards" });
        await flashcardsLink.ClickAsync();
        
        // Wait for the flashcards page to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs * 2); // Extra wait for content to load
    }

    public async Task<bool> IsFlashcardVisibleAsync(string flashcardText)
    {
        // Try multiple approaches to find flashcard text
        var approaches = new[]
        {
            // Direct text match
            () => Page.GetByText(flashcardText),
            
            // Partial text match
            () => Page.Locator($"*:has-text('{flashcardText}')"),
            
            // Look for text within common flashcard containers
            () => Page.Locator($".card:has-text('{flashcardText}')"),
            () => Page.Locator($".flashcard:has-text('{flashcardText}')"),
            () => Page.Locator($".mud-card:has-text('{flashcardText}')"),
            () => Page.Locator($"div:has-text('{flashcardText}')"),
            
            // Look for text within list items or table cells
            () => Page.Locator($"li:has-text('{flashcardText}')"),
            () => Page.Locator($"td:has-text('{flashcardText}')"),
            () => Page.Locator($"tr:has-text('{flashcardText}')"),
            
            // Case-insensitive search
            () => Page.Locator("text=" + flashcardText.ToLower()),
            () => Page.Locator("text=" + flashcardText.ToUpper()),
            
            // Substring search
            () => Page.Locator($"text=/{flashcardText.Substring(0, Math.Min(flashcardText.Length, 10))}/i")
        };

        foreach (var approach in approaches)
        {
            try
            {
                var element = approach();
                if (await element.CountAsync() > 0)
                {
                    return true;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        return false;
    }

    public async Task ClickFlashcardAsync(string flashcardText)
    {
        // Find the flashcard using the same logic as IsFlashcardVisibleAsync
        var approaches = new[]
        {
            () => Page.GetByText(flashcardText),
            () => Page.Locator($"*:has-text('{flashcardText}')"),
            () => Page.Locator($".card:has-text('{flashcardText}')"),
            () => Page.Locator($".flashcard:has-text('{flashcardText}')"),
            () => Page.Locator($".mud-card:has-text('{flashcardText}')"),
            () => Page.Locator($"div:has-text('{flashcardText}')")
        };

        ILocator? flashcardElement = null;
        foreach (var approach in approaches)
        {
            try
            {
                var element = approach();
                if (await element.CountAsync() > 0)
                {
                    flashcardElement = element.First;
                    break;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        if (flashcardElement != null)
        {
            await flashcardElement.ClickAsync();
            await WaitAsync(TestConstants.Timeouts.FormValidationWaitMs);
        }
    }
}
