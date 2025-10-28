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
        // Navigate to flashcards page by clicking the navigation link
        var flashcardsLink = Page.GetByRole(AriaRole.Link, new() { Name = "Flashcards" });
        await flashcardsLink.ClickAsync();
        
        // Wait for the flashcards page to load with correct heading
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the page content to be visible
        var pageHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Your Flashcards" });
        await pageHeading.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs 
        });
        
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task<bool> IsLoadingAsync()
    {
        var loadingSpinner = Page.Locator(".spinner-border");
        return await loadingSpinner.IsVisibleAsync();
    }

    public async Task<bool> HasFlashcardsAsync()
    {
        // Check if there are any flashcards displayed (not in empty state)
        var emptyState = Page.Locator("text=No flashcards yet");
        var hasEmptyState = await emptyState.IsVisibleAsync();
        return !hasEmptyState;
    }

    public async Task<int> GetTotalFlashcardCountAsync()
    {
        // Try multiple selectors to find the total count
        var selectors = new[]
        {
            () => Page.Locator(".card.bg-primary h2"),
            () => Page.Locator(".statistics .total h2"),
            () => Page.Locator("[data-testid='total-count']"),
            () => Page.Locator(".card-header:has-text('Total') + .card-body h2"),
            () => Page.Locator(".badge-primary").Or(Page.Locator(".bg-primary h2"))
        };

        foreach (var selectorFunc in selectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0)
                {
                    var countText = await element.First.TextContentAsync();
                    if (int.TryParse(countText?.Trim(), out var count))
                        return count;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }
        
        return 0; // Default if no count found
    }

    public async Task<int> GetAiGeneratedCountAsync()
    {
        // Try multiple selectors to find the AI count
        var selectors = new[]
        {
            () => Page.Locator(".card.bg-success h2"),
            () => Page.Locator(".statistics .ai h2"),
            () => Page.Locator("[data-testid='ai-count']"),
            () => Page.Locator(".card-header:has-text('AI') + .card-body h2"),
            () => Page.Locator(".badge-success").Or(Page.Locator(".bg-success h2"))
        };

        foreach (var selectorFunc in selectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0)
                {
                    var countText = await element.First.TextContentAsync();
                    if (int.TryParse(countText?.Trim(), out var count))
                        return count;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }
        
        return 0; // Default if no count found
    }

    public async Task<int> GetManualCountAsync()
    {
        // Try multiple selectors to find the manual count
        var selectors = new[]
        {
            () => Page.Locator(".card.bg-info h2"),
            () => Page.Locator(".statistics .manual h2"),
            () => Page.Locator("[data-testid='manual-count']"),
            () => Page.Locator(".card-header:has-text('Manual') + .card-body h2"),
            () => Page.Locator(".badge-info").Or(Page.Locator(".bg-info h2"))
        };

        foreach (var selectorFunc in selectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0)
                {
                    var countText = await element.First.TextContentAsync();
                    if (int.TryParse(countText?.Trim(), out var count))
                        return count;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }
        
        return 0; // Default if no count found
    }

    public async Task ClickFilterAsync(string filterType)
    {
        // Map test filter names to actual UI text
        var actualFilterType = filterType switch
        {
            "Total" => "Total Cards",
            "Ai" => "AI Generated", 
            "AI" => "AI Generated",
            "Manual" => "Manual",
            _ => filterType
        };

        var selectors = actualFilterType switch
        {
            "Total Cards" => new Func<ILocator>[]
            {
                () => Page.Locator(".card.bg-primary"),
                () => Page.GetByText("Total Cards").Locator("..").Locator(".."), // Go up to card element
                () => Page.Locator(".filter-card").Filter(new() { HasText = "Total Cards" })
            },
            "AI Generated" => new Func<ILocator>[]
            {
                () => Page.Locator(".card.bg-success"),
                () => Page.GetByText("AI Generated").Locator("..").Locator(".."), // Go up to card element
                () => Page.Locator(".filter-card").Filter(new() { HasText = "AI Generated" })
            },
            "Manual" => new Func<ILocator>[]
            {
                () => Page.Locator(".card.bg-info"),
                () => Page.GetByText("Manual").Locator("..").Locator(".."), // Go up to card element
                () => Page.Locator(".filter-card").Filter(new() { HasText = "Manual" })
            },
            _ => throw new ArgumentException($"Unknown filter type: {actualFilterType}")
        };

        foreach (var selectorFunc in selectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0)
                {
                    await element.First.ClickAsync();
                    await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
                    return;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }

        throw new InvalidOperationException($"Could not find {actualFilterType} filter with any of the attempted selectors");
    }

    public async Task ClearFilterAsync()
    {
        // Double-click any filter card to clear filter
        var totalCard = Page.Locator(".card.bg-primary");
        await totalCard.DblClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task ToggleViewModeAsync()
    {
        // The toggle button shows "List View" when in card view and "Card View" when in list view
        var toggleButton = Page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("(List View|Card View)") });
        await toggleButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task<bool> IsCardViewActiveAsync()
    {
        // Check if we're in card view by looking for the card structure
        var cardViewElements = Page.Locator(".row .col-12.col-sm-6.col-lg-4");
        var listViewElements = Page.Locator(".list-group .list-group-item");
        
        var hasCardView = await cardViewElements.CountAsync() > 0;
        var hasListView = await listViewElements.CountAsync() > 0;
        
        // If we have cards but no list items, we're in card view
        if (hasCardView && !hasListView) return true;
        if (hasListView && !hasCardView) return false;
        
        // Fallback: check the toggle button text
        try
        {
            var toggleButton = Page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("(List View|Card View)") });
            if (await toggleButton.CountAsync() > 0)
            {
                var buttonText = await toggleButton.TextContentAsync();
                return buttonText?.Contains("List View") == true; // If button says "List View", we're in card view
            }
        }
        catch
        {
            // Continue with default
        }
        
        // Default to card view
        return true;
    }

    public async Task ClickGenerateMoreCardsAsync()
    {
        var generateButton = Page.GetByRole(AriaRole.Link, new() { NameRegex = new System.Text.RegularExpressions.Regex("Generate More Cards") });
        await generateButton.ClickAsync();
    }

    public async Task ClickAddManualCardAsync()
    {
        // Look for the "Create Manually" button as shown in the successful Playwright test
        var addButtonSelectors = new[]
        {
            () => Page.GetByRole(AriaRole.Button, new() { Name = " Create Manually" }),
            () => Page.GetByRole(AriaRole.Button, new() { Name = "Create Manually" }),
            () => Page.GetByRole(AriaRole.Button, new() { Name = "Add Manual Card" }),
            () => Page.Locator("button").Filter(new() { HasText = "Create Manually" }),
            () => Page.Locator("button").Filter(new() { HasText = "Add Manual Card" })
        };

        foreach (var selectorFunc in addButtonSelectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0 && await element.IsVisibleAsync())
                {
                    await element.ClickAsync();
                    await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
                    return;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }

        throw new InvalidOperationException("Could not find Create Manually or Add Manual Card button with any of the attempted selectors");
    }

    public async Task FlipCardAsync(string cardText)
    {
        // Find the card container by text and click the flip button
        var cardContainer = Page.Locator(".card.h-100.shadow").Filter(new() { HasText = cardText });
        var flipButton = cardContainer.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("(Show Question|Show Answer)") });
        await flipButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task<bool> IsCardFlippedAsync(string cardText)
    {
        // Find the card and check if it shows "Show Question" (meaning it's flipped to answer)
        var cardContainer = Page.Locator(".card.h-100.shadow").Filter(new() { HasText = cardText });
        var flipButton = cardContainer.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("(Show Question|Show Answer)") });
        
        if (await flipButton.CountAsync() > 0)
        {
            var buttonText = await flipButton.TextContentAsync();
            return buttonText?.Contains("Show Question") == true;
        }
        
        return false;
    }

    public async Task EditCardAsync(string cardText)
    {
        var cardContainer = Page.Locator(".card, .list-group-item").Filter(new() { HasText = cardText });
        var editButton = cardContainer.GetByRole(AriaRole.Button, new() { Name = "Edit" }).Or(cardContainer.Locator("button[title='Edit']"));
        await editButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task DeleteCardAsync(string cardText)
    {
        // Find the card container and click the delete button (trash icon)
        var cardContainer = Page.Locator(".card.h-100.shadow, .list-group-item").Filter(new() { HasText = cardText });
        var deleteButton = cardContainer.Locator("button[title='Delete']");
        await deleteButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task ConfirmDeleteAsync()
    {
        var confirmButton = Page.Locator(".modal .btn-danger");
        await confirmButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task CancelDeleteAsync()
    {
        var cancelButton = Page.Locator(".modal .btn-secondary");
        await cancelButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task EnterQuestionAsync(string question)
    {
        // Use the exact selector from the successful Playwright test
        var questionField = Page.GetByRole(AriaRole.Textbox, new() { Name = "Question (Front)*" });
        await questionField.ClickAsync();
        await questionField.FillAsync(question);
    }

    public async Task EnterAnswerAsync(string answer)
    {
        // Use the exact selector from the successful Playwright test
        var answerField = Page.GetByRole(AriaRole.Textbox, new() { Name = "Answer (Back)*" });
        await answerField.ClickAsync();
        await answerField.FillAsync(answer);
    }

    public async Task EnterTagsAsync(string tags)
    {
        // Use the exact selector from the successful Playwright test
        var tagsField = Page.GetByRole(AriaRole.Textbox, new() { Name = "Tags (optional)" });
        await tagsField.ClickAsync();
        await tagsField.FillAsync(tags);
    }

    public async Task ClickCreateCardAsync()
    {
        // Use the exact selector from the successful Playwright test
        var createButton = Page.GetByRole(AriaRole.Button, new() { Name = " Create Card" });
        await createButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task CancelCreateCardAsync()
    {
        var cancelButton = Page.Locator(".modal .btn-secondary");
        await cancelButton.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }

    public async Task<bool> IsCreateModalVisibleAsync()
    {
        var modal = Page.Locator(".modal").Filter(new() { HasText = "Create Manual Flashcard" });
        return await modal.IsVisibleAsync();
    }

    public async Task<bool> IsDeleteModalVisibleAsync()
    {
        var modal = Page.Locator(".modal").Filter(new() { HasText = "Confirm Deletion" });
        return await modal.IsVisibleAsync();
    }

    public async Task<string?> GetCreateErrorAsync()
    {
        var errorElement = Page.Locator(".modal .text-danger, .modal .alert-danger");
        if (await errorElement.IsVisibleAsync())
        {
            return await errorElement.TextContentAsync();
        }
        return null;
    }

    public async Task<bool> IsFlashcardVisibleAsync(string flashcardText)
    {
        var flashcard = Page.Locator(".card, .list-group-item").Filter(new() { HasText = flashcardText });
        return await flashcard.IsVisibleAsync();
    }

    public async Task ClickFlashcardAsync(string flashcardText)
    {
        var flashcard = Page.Locator(".card, .list-group-item").Filter(new() { HasText = flashcardText });
        await flashcard.ClickAsync();
        await WaitAsync(TestConstants.Timeouts.DefaultWaitMs);
    }
}
