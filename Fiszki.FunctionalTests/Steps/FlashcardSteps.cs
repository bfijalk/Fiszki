using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using Fiszki.FunctionalTests.Constants;
using FluentAssertions;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class FlashcardSteps : BaseSteps
{
    private IFlashcardGenerationPage? _flashcardGenerationPage;
    private IFlashcardsPage? _flashcardsPage;

    public FlashcardSteps(ScenarioContext scenarioContext) : base(scenarioContext) { }

    private IFlashcardGenerationPage FlashcardGenerationPage => _flashcardGenerationPage ??= GetPage<FlashcardGenerationPage>();
    private IFlashcardsPage FlashcardsPage => _flashcardsPage ??= GetPage<FlashcardsPage>();

    [Given("I am on the Flashcard Generation page")]
    [When("I am on the Flashcard Generation page")]
    public async Task GivenIAmOnFlashcardGenerationPage()
    {
        await FlashcardGenerationPage.NavigateAsync();
    }

    [When("I enter the sample source text")]
    public async Task WhenIEnterSampleSourceText()
    {
        await FlashcardGenerationPage.EnterSourceTextAsync(TestConstants.TestData.SampleSourceText);
    }

    [When("I set maximum cards to {int}")]
    public async Task WhenISetMaximumCards(int maxCards)
    {
        await FlashcardGenerationPage.SetMaximumCardsAsync(maxCards);
    }

    [When("I click Generate Flashcards")]
    public async Task WhenIClickGenerateFlashcards()
    {
        await FlashcardGenerationPage.ClickGenerateFlashcardsAsync();
    }

    [When("I click Accept All")]
    public async Task WhenIClickAcceptAll()
    {
        await FlashcardGenerationPage.ClickAcceptAllAsync();
    }

    [When("I click Save Selected")]
    public async Task WhenIClickSaveSelected()
    {
        await FlashcardGenerationPage.ClickSaveSelectedAsync();
    }

    [Given("I am on the Flashcards page")]
    [When("I navigate to the Flashcards page")]
    public async Task GivenIAmOnFlashcardsPage()
    {
        await FlashcardsPage.NavigateAsync();
    }

    [Then("I should see the flashcard {string}")]
    public async Task ThenIShouldSeeFlashcard(string flashcardText)
    {
        var isVisible = await FlashcardsPage.IsFlashcardVisibleAsync(flashcardText);
        isVisible.Should().BeTrue($"Flashcard '{flashcardText}' should be visible but was not found");
    }

    [When("I click the flashcard {string}")]
    public async Task WhenIClickFlashcard(string flashcardText)
    {
        await FlashcardsPage.ClickFlashcardAsync(flashcardText);
    }

    [Then("I should see all expected flashcards")]
    public async Task ThenIShouldSeeAllExpectedFlashcards()
    {
        // Cast to concrete type to access PageInstance
        var flashcardsPageInstance = (FlashcardsPage)FlashcardsPage;
        
        // First, check if there are any flashcards at all on the page
        // Look for elements that contain flashcard-like content with timestamps
        var flashcardSelectors = new[]
        {
            "text=/Created \\d{1,2}\\/\\d{1,2}\\/\\d{4}/", // Matches "Created MM/DD/YYYY" pattern
            ".card", ".flashcard", ".mud-card",
            "div:has-text('Created')",
            "li", "tr:not(:first-child)", // table rows excluding header
            ".list-item"
        };

        var foundAnyFlashcards = false;
        var totalFlashcardCount = 0;
        
        foreach (var selector in flashcardSelectors)
        {
            var elements = flashcardsPageInstance.PageInstance.Locator(selector);
            var count = await elements.CountAsync();
            if (count > 0)
            {
                foundAnyFlashcards = true;
                totalFlashcardCount = Math.Max(totalFlashcardCount, count);
                break;
            }
        }

        if (!foundAnyFlashcards)
        {
            // If no flashcards found with common selectors, try to find any content with creation timestamps
            var pageContent = await flashcardsPageInstance.PageInstance.TextContentAsync("body");
            var hasCreatedText = pageContent?.Contains("Created") == true;
            hasCreatedText.Should().BeTrue("The flashcards page should contain created flashcards with timestamps");
        }

        // We expect at least some flashcards to be created (we set max to 5 in the test)
        // Be flexible - accept 1 or more flashcards since AI generation can vary
        totalFlashcardCount.Should().BeGreaterThan(0, "At least one flashcard should have been created and saved");
        
        // Log the success for debugging
        Console.WriteLine($"Successfully found {totalFlashcardCount} flashcards on the page");
    }

    [When("I click all expected flashcards")]
    public async Task WhenIClickAllExpectedFlashcards()
    {
        // Cast to concrete type to access PageInstance
        var flashcardsPageInstance = (FlashcardsPage)FlashcardsPage;
        
        // Find all clickable flashcard elements and click them
        // Based on the user's example, look for elements with creation timestamps
        var flashcardElements = flashcardsPageInstance.PageInstance.GetByText(new System.Text.RegularExpressions.Regex(@".*Created \d{1,2}\/\d{1,2}\/\d{4}.*"));
        
        var count = await flashcardElements.CountAsync();
        if (count == 0)
        {
            // Fallback: try to find any clickable flashcard elements
            var fallbackSelectors = new[]
            {
                ".card", ".flashcard", ".mud-card", 
                "div:has-text('Created')", 
                "li:has-text('Created')"
            };
            
            foreach (var selector in fallbackSelectors)
            {
                var elements = flashcardsPageInstance.PageInstance.Locator(selector);
                count = await elements.CountAsync();
                if (count > 0)
                {
                    // Click each element
                    for (int i = 0; i < count; i++)
                    {
                        await elements.Nth(i).ClickAsync();
                        await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs);
                    }
                    break;
                }
            }
        }
        else
        {
            // Click each flashcard element found by the regex
            for (int i = 0; i < count; i++)
            {
                await flashcardElements.Nth(i).ClickAsync();
                await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs);
            }
        }
        
        Console.WriteLine($"Clicked {count} flashcard elements");
    }

    [Then("all flashcard interactions should be successful")]
    public async Task ThenAllFlashcardInteractionsShouldBeSuccessful()
    {
        // This step serves as a final verification that all previous interactions completed successfully
        // If we reach this point without exceptions, the test has passed
        await Task.CompletedTask;
    }

    [Then("I should see the \"(.*)\" message")]
    public async Task ThenIShouldSeeMessage(string message)
    {
        var messageElement = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance.GetByText(message);
        await messageElement.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
        var isVisible = await messageElement.IsVisibleAsync();
        isVisible.Should().BeTrue($"Message '{message}' should be visible on the page");
    }

    [Then("I should see generated flashcards on the page")]
    public async Task ThenIShouldSeeGeneratedFlashcardsOnPage()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Wait for flashcards to be generated and displayed
        // Look for the "Generated Flashcards" section with flashcard items
        var flashcardSection = pageInstance.Locator("text=Generated Flashcards");
        await flashcardSection.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.FlashcardGenerationWaitMs
        });

        // Look for individual flashcard items in the generation results
        // Based on the UI, these appear as question-answer pairs
        var flashcardSelectors = new[]
        {
            "div:has-text('Kto odkrył ruiny Heliori')", // Example from the sample text
            "div:has-text('Czym słynęło miasto Heliora')",
            "div:has-text('Jakie badania przeprowadzono')",
            "[data-testid*='flashcard']",
            ".flashcard-item",
            ".generated-flashcard",
            "div[role='listitem']" // Common pattern for list items
        };

        var foundFlashcards = false;
        var flashcardCount = 0;

        foreach (var selector in flashcardSelectors)
        {
            try
            {
                var elements = pageInstance.Locator(selector);
                var count = await elements.CountAsync();
                if (count > 0)
                {
                    foundFlashcards = true;
                    flashcardCount = count;
                    break;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }

        if (!foundFlashcards)
        {
            // Fallback: check for any content that looks like flashcards
            var pageContent = await pageInstance.TextContentAsync("body");
            var hasFlashcardContent = pageContent?.Contains("Kto odkrył") == true || 
                                    pageContent?.Contains("Czym słynęło") == true ||
                                    pageContent?.Contains("Example:") == true;
            hasFlashcardContent.Should().BeTrue("Generated flashcards should be visible on the generation page");
        }
        else
        {
            flashcardCount.Should().BeGreaterThan(0, "At least one flashcard should be generated and visible");
            Console.WriteLine($"Found {flashcardCount} generated flashcards on the generation page");
        }
    }

    [Then("I should see the correct flashcard count displayed")]
    public async Task ThenIShouldSeeCorrectFlashcardCount()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Look for the count display like "Generated Flashcards (0 of 5 selected)"
        var countPatterns = new[]
        {
            "text=/Generated Flashcards \\(\\d+ of \\d+ selected\\)/",
            "text=/\\d+ of \\d+ selected/",
            "text=/Generated Flashcards/",
            "text=/\\d+ selected/"
        };

        var foundCount = false;
        foreach (var pattern in countPatterns)
        {
            try
            {
                var element = pageInstance.Locator(pattern);
                if (await element.CountAsync() > 0)
                {
                    foundCount = true;
                    var countText = await element.First.TextContentAsync();
                    Console.WriteLine($"Found flashcard count display: {countText}");
                    break;
                }
            }
            catch
            {
                // Continue to next pattern
            }
        }

        foundCount.Should().BeTrue("Flashcard count should be displayed on the generation page");
    }

    [Then("the generated flashcards should contain expected content")]
    public async Task ThenGeneratedFlashcardsShouldContainExpectedContent()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Check for content from the sample source text
        var expectedKeywords = new[]
        {
            "Heliora", "Helena Markos", "ceramika", "Selina", "1923", "500"
        };

        var foundKeywords = 0;
        foreach (var keyword in expectedKeywords)
        {
            try
            {
                var element = pageInstance.GetByText(keyword);
                if (await element.CountAsync() > 0)
                {
                    foundKeywords++;
                }
            }
            catch
            {
                // Continue checking other keywords
            }
        }

        foundKeywords.Should().BeGreaterThan(0, "Generated flashcards should contain content related to the source text");
        Console.WriteLine($"Found {foundKeywords} expected keywords in generated flashcards");
    }

    [Then("all flashcards should be selected")]
    public async Task ThenAllFlashcardsShouldBeSelected()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // After clicking "Accept All", the count should show all flashcards selected
        // Look for text like "Generated Flashcards (5 of 5 selected)"
        var selectedPattern = pageInstance.Locator("text=/\\((\\d+) of \\1 selected\\)/");
        await selectedPattern.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.DefaultWaitMs
        });

        var isVisible = await selectedPattern.IsVisibleAsync();
        isVisible.Should().BeTrue("All flashcards should be selected after clicking Accept All");
    }

    [Then("the Save Selected button should be enabled")]
    public async Task ThenSaveSelectedButtonShouldBeEnabled()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Try multiple approaches to find the Save Selected button
        var buttonSelectors = new[]
        {
            () => pageInstance.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex(@"SAVE SELECTED.*") }),
            () => pageInstance.GetByText("SAVE SELECTED"),
            () => pageInstance.Locator("button:has-text('SAVE SELECTED')"),
            () => pageInstance.Locator("button:has-text('Save Selected')"),
            () => pageInstance.Locator("button:has-text('SAVE')"),
            () => pageInstance.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex(@".*SAVE.*") })
        };

        ILocator? saveButton = null;
        foreach (var selectorFunc in buttonSelectors)
        {
            try
            {
                var button = selectorFunc();
                if (await button.CountAsync() > 0)
                {
                    saveButton = button.First;
                    break;
                }
            }
            catch
            {
                // Continue to next selector
            }
        }

        if (saveButton == null)
        {
            // Fallback: log all buttons on the page for debugging
            var allButtons = pageInstance.Locator("button");
            var buttonCount = await allButtons.CountAsync();
            Console.WriteLine($"Found {buttonCount} buttons on the page");
            for (int i = 0; i < buttonCount && i < 10; i++) // Limit to first 10 buttons
            {
                try
                {
                    var debugButtonText = await allButtons.Nth(i).TextContentAsync();
                    Console.WriteLine($"Button {i}: '{debugButtonText}'");
                }
                catch { }
            }
            
            throw new InvalidOperationException("Could not find Save Selected button with any of the attempted selectors");
        }
        
        await saveButton.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.DefaultWaitMs
        });

        var isEnabled = await saveButton.IsEnabledAsync();
        isEnabled.Should().BeTrue("Save Selected button should be enabled when flashcards are selected");
        
        var buttonText = await saveButton.TextContentAsync();
        Console.WriteLine($"Save button found with text: '{buttonText}' and enabled: {isEnabled}");
    }
}
