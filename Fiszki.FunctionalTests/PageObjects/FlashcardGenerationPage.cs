using Microsoft.Playwright;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class FlashcardGenerationPage : BasePage, IFlashcardGenerationPage
{
    public FlashcardGenerationPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    // Expose the Page property for step access
    public IPage PageInstance => Page;

    public async Task NavigateAsync()
    {
        // Use the proper base URL and route constants instead of hardcoded URL
        await NavigateToAsync(TestConstants.Routes.Generate);
        await WaitAsync();
    }

    public async Task EnterSourceTextAsync(string text)
    {
        // Look for the Source Text textarea - try simple selectors first
        var selectors = new[]
        {
            "textarea", // Most likely match - there should only be one textarea on the page
            "textarea[placeholder*='text']",
            "textarea[rows]",
            ".form-control textarea",
            ".mud-input-slot textarea"
        };

        ILocator? sourceTextElement = null;
        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                sourceTextElement = element.First;
                break;
            }
        }

        // If still not found, try the fallback selector (but fix the syntax)
        if (sourceTextElement == null)
        {
            sourceTextElement = Page.Locator("textarea").First;
        }

        await sourceTextElement.ClickAsync();
        await sourceTextElement.FillAsync(text);
    }

    public async Task SetMaximumCardsAsync(int maxCards)
    {
        // Based on the UI, this appears to be a number input field
        var selectors = new[]
        {
            "input[type='number']", // Most likely match
            "input:near(:text('Maximum Cards'))",
            ".form-control[type='number']",
            "input[value='20']" // Default value appears to be 20
        };

        ILocator? maxCardsElement = null;
        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                maxCardsElement = element.First;
                break;
            }
        }

        // Fallback: try to find by label or nearby text
        if (maxCardsElement == null)
        {
            // Look for input near "Maximum Cards" text
            maxCardsElement = Page.Locator("label:has-text('Maximum Cards') + input, label:has-text('Maximum Cards') ~ input").First;
        }

        await maxCardsElement.ClickAsync();
        await maxCardsElement.SelectTextAsync(); // Select all existing text
        await maxCardsElement.FillAsync(maxCards.ToString()); // Use FillAsync instead of TypeAsync
    }

    public async Task ClickGenerateFlashcardsAsync()
    {
        // Look for the Generate Flashcards button
        var selectors = new[]
        {
            "button:has-text('Generate Flashcards')", // Exact match from UI
            ".btn:has-text('Generate')",
            "button:has-text('Generate')",
            "input[type='submit'][value*='Generate']"
        };

        ILocator? generateButton = null;
        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                generateButton = element.First;
                break;
            }
        }

        // Fallback to role-based selector
        generateButton ??= Page.GetByRole(AriaRole.Button, new() { Name = TestConstants.Labels.GenerateFlashcards });

        await generateButton.ClickAsync();
        
        // Wait for generation to complete - longer timeout for AI processing
        await Page.WaitForTimeoutAsync(TestConstants.Timeouts.FlashcardGenerationWaitMs);
    }

    public async Task ClickAcceptAllAsync()
    {
        // After generation, look for accept/select all buttons
        var selectors = new[]
        {
            "button:has-text('Accept All')",
            "button:has-text('Select All')",
            ".btn:has-text('Accept')",
            "input[type='checkbox']:near(:text('Select All')) + label"
        };

        ILocator? acceptAllButton = null;
        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                acceptAllButton = element.First;
                break;
            }
        }

        // Fallback to role-based selector
        acceptAllButton ??= Page.GetByRole(AriaRole.Button, new() { Name = TestConstants.Labels.AcceptAll });

        await acceptAllButton.ClickAsync();
        await WaitAsync();
    }

    public async Task ClickSaveSelectedAsync()
    {
        // Look for save button
        var selectors = new[]
        {
            "button:has-text('Save Selected')",
            "button:has-text('Save')",
            ".btn:has-text('Save')",
            "input[type='submit'][value*='Save']"
        };

        ILocator? saveButton = null;
        foreach (var selector in selectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                saveButton = element.First;
                break;
            }
        }

        // Fallback to role-based selector with regex for flexibility
        saveButton ??= Page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex(@"Save.*") });

        await saveButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs * 2); // Extra wait for save operation
    }
}
