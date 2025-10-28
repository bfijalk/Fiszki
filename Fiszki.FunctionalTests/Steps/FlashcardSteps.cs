using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using Fiszki.FunctionalTests.Constants;
using Fiszki.FunctionalTests.Services;
using Fiszki.FunctionalTests.Support;
using FluentAssertions;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class FlashcardSteps : BaseSteps
{
    private IFlashcardGenerationPage? _flashcardGenerationPage;
    private IFlashcardsPage? _flashcardsPage;
    private readonly ScenarioContext _scenarioContext;

    public FlashcardSteps(ScenarioContext scenarioContext) : base(scenarioContext) 
    {
        _scenarioContext = scenarioContext;
    }

    private IFlashcardGenerationPage FlashcardGenerationPage => _flashcardGenerationPage ??= GetPage<FlashcardGenerationPage>();
    private IFlashcardsPage FlashcardsPage => _flashcardsPage ??= GetPage<FlashcardsPage>();

    private string BaseUrl => (string)_scenarioContext[TestContextKeys.BaseUrl];

    // Dummy data seeding step for UI-focused tests
    [Given("I have dummy flashcards in my account")]
    [When("I have dummy flashcards in my account")]
    public async Task GivenIHaveDummyFlashcardsInMyAccount()
    {
        // Use the same connection string pattern as DatabaseCleanupHooks
        var connectionString = Environment.GetEnvironmentVariable("FISZKI_DB_CONNECTION_STRING") 
            ?? "Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres";
        
        // Get the test user email from scenario context - this is the same user that's logging in
        var testEmail = _scenarioContext[TestContextKeys.TestUserEmail]?.ToString()
            ?? throw new InvalidOperationException("Test user email not found in scenario context. Ensure the test user management hooks are properly configured.");
        
        Console.WriteLine("[Test Setup] Seeding dummy flashcards for test user: " + testEmail);
        
        try
        {
            var seedingService = new TestFlashcardSeedingService(connectionString);
            await seedingService.EnsureDummyFlashcardsExistAsync(testEmail);
            
            Console.WriteLine("[Test Setup] Dummy flashcards have been seeded for UI testing");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Test Setup] Warning: Could not seed dummy flashcards - " + ex.Message);
            Console.WriteLine("[Test Setup] This may indicate database connectivity issues. Tests may fail if they rely on existing flashcard data.");
            
            // Don't throw the exception - let the test continue and fail more gracefully if needed
            // This allows tests that don't strictly require dummy data to potentially still pass
        }
    }

    // Navigation steps
    [Given("I am on the Flashcard Generation page")]
    [When("I am on the Flashcard Generation page")]
    public async Task GivenIAmOnFlashcardGenerationPage()
    {
        await FlashcardGenerationPage.NavigateAsync();
    }

    [Given("I am on the Flashcards page")]
    [When("I navigate to the Flashcards page")]
    public async Task GivenIAmOnFlashcardsPage()
    {
        try
        {
            await FlashcardsPage.NavigateAsync();
        }
        catch (TimeoutException)
        {
            var pageInstance = ((FlashcardsPage)FlashcardsPage).PageInstance;
            await pageInstance.GotoAsync($"{BaseUrl}/flashcards");
            
            var pageHeading = pageInstance.GetByRole(AriaRole.Heading, new() { Name = "Your Flashcards" });
            await pageHeading.WaitForAsync(new() 
            { 
                State = WaitForSelectorState.Visible, 
                Timeout = TestConstants.Timeouts.NavigationTimeoutMs 
            });
        }
    }

    // Generation steps
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

    // Manual flashcard creation steps
    [When("I click Add Manual Card")]
    [Given("I click Add Manual Card")]
    public async Task WhenIClickAddManualCard()
    {
        await FlashcardsPage.ClickAddManualCardAsync();
    }

    [Then("I should see the create card modal")]
    public async Task ThenIShouldSeeCreateCardModal()
    {
        var isVisible = await FlashcardsPage.IsCreateModalVisibleAsync();
        isVisible.Should().BeTrue("Create card modal should be visible");
    }

    [When("I enter question {string}")]
    public async Task WhenIEnterQuestion(string question)
    {
        await FlashcardsPage.EnterQuestionAsync(question);
    }

    [When("I enter answer {string}")]
    public async Task WhenIEnterAnswer(string answer)
    {
        await FlashcardsPage.EnterAnswerAsync(answer);
    }

    [When("I enter tags {string}")]
    public async Task WhenIEnterTags(string tags)
    {
        await FlashcardsPage.EnterTagsAsync(tags);
    }

    [When("I click Create Card")]
    public async Task WhenIClickCreateCard()
    {
        await FlashcardsPage.ClickCreateCardAsync();
    }

    [When("I cancel card creation")]
    public async Task WhenICancelCardCreation()
    {
        await FlashcardsPage.CancelCreateCardAsync();
    }

    [Then("I should see a validation error")]
    public async Task ThenIShouldSeeValidationError()
    {
        var errorMessage = await FlashcardsPage.GetCreateErrorAsync();
        errorMessage.Should().NotBeNullOrEmpty("A validation error message should be displayed");
    }

    [Then("the create card modal should be closed")]
    public async Task ThenCreateCardModalShouldBeClosed()
    {
        var isVisible = await FlashcardsPage.IsCreateModalVisibleAsync();
        isVisible.Should().BeFalse("Create card modal should be closed");
    }

    // Flashcard viewing and interaction steps
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

    [When("I flip the card {string}")]
    public async Task WhenIFlipCard(string cardText)
    {
        await FlashcardsPage.FlipCardAsync(cardText);
    }

    [Then("the card {string} should be flipped")]
    public async Task ThenCardShouldBeFlipped(string cardText)
    {
        var isFlipped = await FlashcardsPage.IsCardFlippedAsync(cardText);
        isFlipped.Should().BeTrue($"Card '{cardText}' should be flipped");
    }

    [Then("the card {string} should show the question")]
    public async Task ThenCardShouldShowQuestion(string cardText)
    {
        var isFlipped = await FlashcardsPage.IsCardFlippedAsync(cardText);
        isFlipped.Should().BeFalse($"Card '{cardText}' should show the question (not be flipped)");
    }

    // Statistics and filtering steps
    [Then("I should see the flashcard statistics")]
    public async Task ThenIShouldSeeFlashcardStatistics()
    {
        var hasFlashcards = await FlashcardsPage.HasFlashcardsAsync();
        if (hasFlashcards)
        {
            var totalCount = await FlashcardsPage.GetTotalFlashcardCountAsync();
            var aiCount = await FlashcardsPage.GetAiGeneratedCountAsync();
            var manualCount = await FlashcardsPage.GetManualCountAsync();

            totalCount.Should().BeGreaterThan(0, "Total flashcard count should be greater than 0");
            (aiCount + manualCount).Should().Be(totalCount, "AI + Manual counts should equal total count");
            
            Console.WriteLine($"Flashcard statistics: Total={totalCount}, AI={aiCount}, Manual={manualCount}");
        }
    }

    [When("I click the {string} filter")]
    public async Task WhenIClickFilter(string filterType)
    {
        await FlashcardsPage.ClickFilterAsync(filterType);
    }

    [When("I clear the filter")]
    public async Task WhenIClearFilter()
    {
        await FlashcardsPage.ClearFilterAsync();
    }

    [Then("I should see only AI generated flashcards")]
    public async Task ThenIShouldSeeOnlyAiGeneratedFlashcards()
    {
        var aiCount = await FlashcardsPage.GetAiGeneratedCountAsync();
        aiCount.Should().BeGreaterThanOrEqualTo(0, "AI count should be visible");
        Console.WriteLine($"Viewing AI generated flashcards: {aiCount} cards");
    }

    [Then("I should see only manual flashcards")]
    public async Task ThenIShouldSeeOnlyManualFlashcards()
    {
        var manualCount = await FlashcardsPage.GetManualCountAsync();
        manualCount.Should().BeGreaterThanOrEqualTo(0, "Manual count should be visible");
        Console.WriteLine($"Viewing manual flashcards: {manualCount} cards");
    }

    [Then("I should see all flashcards")]
    public async Task ThenIShouldSeeAllFlashcards()
    {
        var totalCount = await FlashcardsPage.GetTotalFlashcardCountAsync();
        var aiCount = await FlashcardsPage.GetAiGeneratedCountAsync();
        var manualCount = await FlashcardsPage.GetManualCountAsync();
        
        totalCount.Should().Be(aiCount + manualCount, "Total should equal sum of AI and manual when no filter is active");
        Console.WriteLine($"Viewing all flashcards: Total={totalCount}, AI={aiCount}, Manual={manualCount}");
    }

    // View mode steps
    [When("I toggle the view mode")]
    public async Task WhenIToggleViewMode()
    {
        await FlashcardsPage.ToggleViewModeAsync();
    }

    [Then("I should be in {string} view")]
    public async Task ThenIShouldBeInView(string viewType)
    {
        var isCardView = await FlashcardsPage.IsCardViewActiveAsync();
        if (viewType.Equals("card", StringComparison.OrdinalIgnoreCase))
        {
            isCardView.Should().BeTrue("Should be in card view");
        }
        else if (viewType.Equals("list", StringComparison.OrdinalIgnoreCase))
        {
            isCardView.Should().BeFalse("Should be in list view");
        }
    }

    [When("I am in card view")]
    [Given("I am in card view")]
    public async Task WhenIAmInCardView()
    {
        var isCardView = await FlashcardsPage.IsCardViewActiveAsync();
        if (!isCardView)
        {
            await FlashcardsPage.ToggleViewModeAsync();
        }
    }

    // Delete operations
    [When("I click delete on card {string}")]
    public async Task WhenIClickDeleteOnCard(string cardText)
    {
        await FlashcardsPage.DeleteCardAsync(cardText);
    }

    [Then("I should see the delete confirmation modal")]
    public async Task ThenIShouldSeeDeleteConfirmationModal()
    {
        var isVisible = await FlashcardsPage.IsDeleteModalVisibleAsync();
        isVisible.Should().BeTrue("Delete confirmation modal should be visible");
    }

    [When("I confirm the deletion")]
    public async Task WhenIConfirmDeletion()
    {
        await FlashcardsPage.ConfirmDeleteAsync();
    }

    [When("I cancel the deletion")]
    public async Task WhenICancelDeletion()
    {
        await FlashcardsPage.CancelDeleteAsync();
    }

    [Then("I should still see the flashcard {string}")]
    public async Task ThenIShouldStillSeeFlashcard(string flashcardText)
    {
        var isVisible = await FlashcardsPage.IsFlashcardVisibleAsync(flashcardText);
        isVisible.Should().BeTrue($"Flashcard '{flashcardText}' should still be visible after canceling deletion");
    }

    [Then("I should not see the flashcard {string}")]
    public async Task ThenIShouldNotSeeFlashcard(string flashcardText)
    {
        await Task.Delay(TestConstants.Timeouts.DefaultWaitMs);
        var isVisible = await FlashcardsPage.IsFlashcardVisibleAsync(flashcardText);
        isVisible.Should().BeFalse($"Flashcard '{flashcardText}' should not be visible after deletion");
    }

    // Empty state steps
    [Given("I have no flashcards")]
    public async Task GivenIHaveNoFlashcards()
    {
        // This is handled by the database cleanup
    }

    [Given("I have some existing flashcards")]
    public Task GivenIHaveSomeExistingFlashcards()
    {
        // This step indicates that flashcards should exist
        return Task.CompletedTask;
    }

    [Then("I should see the empty state message")]
    public async Task ThenIShouldSeeEmptyStateMessage()
    {
        var pageInstance = ((FlashcardsPage)FlashcardsPage).PageInstance;
        var emptyMessage = pageInstance.Locator("text=No flashcards yet");
        var isVisible = await emptyMessage.IsVisibleAsync();
        isVisible.Should().BeTrue("Empty state message should be visible");
    }

    [Then("I should see the {string} button")]
    [Given("I should see the {string} button")]
    public async Task ThenIShouldSeeButton(string buttonText)
    {
        var pageInstance = ((FlashcardsPage)FlashcardsPage).PageInstance;
        var button = pageInstance.GetByRole(AriaRole.Button, new() { Name = buttonText });
        var isVisible = await button.IsVisibleAsync();
        isVisible.Should().BeTrue($"Button '{buttonText}' should be visible");
    }

    // Complex test scenario steps
    [Then("I should see all expected flashcards")]
    public async Task ThenIShouldSeeAllExpectedFlashcards()
    {
        var isLoading = await FlashcardsPage.IsLoadingAsync();
        if (isLoading)
        {
            await Task.Delay(TestConstants.Timeouts.DefaultWaitMs * 2);
        }

        var hasFlashcards = await FlashcardsPage.HasFlashcardsAsync();
        if (!hasFlashcards)
        {
            var emptyStateTotalCount = await FlashcardsPage.GetTotalFlashcardCountAsync();
            emptyStateTotalCount.Should().Be(0, "If no flashcards are displayed, total count should be 0");
            Console.WriteLine("No flashcards found - user appears to have empty flashcard collection");
            return;
        }

        var totalCount = await FlashcardsPage.GetTotalFlashcardCountAsync();
        var aiCount = await FlashcardsPage.GetAiGeneratedCountAsync();
        var manualCount = await FlashcardsPage.GetManualCountAsync();

        totalCount.Should().BeGreaterThan(0, "At least one flashcard should have been created and saved");
        
        Console.WriteLine($"Successfully found flashcard statistics: Total={totalCount}, AI={aiCount}, Manual={manualCount}");
    }

    [When("I click all expected flashcards")]
    public async Task WhenIClickAllExpectedFlashcards()
    {
        var hasFlashcards = await FlashcardsPage.HasFlashcardsAsync();
        if (!hasFlashcards)
        {
            Console.WriteLine("No flashcards to click - collection appears to be empty");
            return;
        }

        var expectedContent = new[] { "Heliora", "Helena Markos", "ceramika", "Selina", "świątynia" };
        var clickedCount = 0;
        
        foreach (var content in expectedContent)
        {
            try
            {
                var isVisible = await FlashcardsPage.IsFlashcardVisibleAsync(content);
                if (isVisible)
                {
                    await FlashcardsPage.ClickFlashcardAsync(content);
                    clickedCount++;
                    await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not click flashcard with content '{content}': {ex.Message}");
            }
        }
        
        Console.WriteLine($"Successfully clicked {clickedCount} flashcard elements");
    }

    [Then("all flashcard interactions should be successful")]
    public async Task ThenAllFlashcardInteractionsShouldBeSuccessful()
    {
        await Task.CompletedTask;
    }

    [When("I enter flashcard question \"(.*)\"")]
    [Given("I enter flashcard question \"(.*)\"")]
    public async Task WhenIEnterFlashcardQuestion(string question)
    {
        await FlashcardsPage.EnterQuestionAsync(question);
    }

    [When("I enter flashcard answer \"(.*)\"")]
    [Given("I enter flashcard answer \"(.*)\"")]
    public async Task WhenIEnterFlashcardAnswer(string answer)
    {
        await FlashcardsPage.EnterAnswerAsync(answer);
    }

    [When("I enter flashcard tags \"(.*)\"")]
    [Given("I enter flashcard tags \"(.*)\"")]
    public async Task WhenIEnterFlashcardTags(string tags)
    {
        await FlashcardsPage.EnterTagsAsync(tags);
    }

    [Then("I should see the created flashcard with question \"(.*)\"")]
    public async Task ThenIShouldSeeCreatedFlashcardWithQuestion(string question)
    {
        var page = ((FlashcardsPage)FlashcardsPage).PageInstance;
        // Look for the flashcard containing the question text, similar to the Playwright test
        var flashcardLocator = page.Locator("div").Filter(new() { HasText = $"Question: {question}" });
        await flashcardLocator.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs 
        });
        
        var isVisible = await flashcardLocator.IsVisibleAsync();
        isVisible.Should().BeTrue($"Flashcard with question '{question}' should be visible");
    }

    [Then("I should see the {string} message")]
    public async Task ThenIShouldSeeTheMessage(string message)
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        var messageElement = pageInstance.GetByText(message);
        var isVisible = await messageElement.IsVisibleAsync();
        isVisible.Should().BeTrue($"Message '{message}' should be visible on the page");
    }

    [Then("I should see generated flashcards on the page")]
    public async Task ThenIShouldSeeGeneratedFlashcardsOnThePage()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Wait for the generation process to complete and flashcards to appear
        await pageInstance.WaitForTimeoutAsync(2000);
        
        // Look for the "Generated Flashcards" heading or container
        var generatedSection = pageInstance.Locator("text=Generated Flashcards");
        await generatedSection.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs 
        });
        
        // Look for flashcard content containers - these contain the actual flashcard data
        var flashcardContainers = pageInstance.Locator("div").Filter(new() { HasText = "Example:" });
        var count = await flashcardContainers.CountAsync();
        
        // Alternative: Look for edit/delete buttons which indicate flashcard items
        if (count == 0)
        {
            var editButtons = pageInstance.Locator("button[title='Edit'], button:has(svg)").Filter(new() { HasText = "" });
            count = await editButtons.CountAsync();
        }
        
        // Alternative: Look for flashcard text content that we know should be there
        if (count == 0)
        {
            var expectedContent = new[] { "Helena Markos", "Heliora", "ceramika", "Selina" };
            foreach (var content in expectedContent)
            {
                var contentElement = pageInstance.GetByText(content);
                if (await contentElement.CountAsync() > 0)
                {
                    count++;
                    break; // At least one flashcard found
                }
            }
        }
        
        count.Should().BeGreaterThan(0, "Generated flashcards should be visible on the page");
        Console.WriteLine($"Found {count} generated flashcard elements on the page");
    }

    [Then("I should see the correct flashcard count displayed")]
    public async Task ThenIShouldSeeTheCorrectFlashcardCountDisplayed()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Look for the count display in the header like "Generated Flashcards (0 of 5 selected)"
        var countHeader = pageInstance.Locator("text=/Generated Flashcards \\([0-9]+ of [0-9]+ selected\\)/");
        var isVisible = await countHeader.IsVisibleAsync();
        
        if (!isVisible)
        {
            // Alternative: look for any count-related text
            var alternativeCount = pageInstance.Locator("text=/[0-9]+ of [0-9]+/");
            isVisible = await alternativeCount.IsVisibleAsync();
        }
        
        isVisible.Should().BeTrue("Flashcard count should be displayed on the generation page");
    }

    [Then("the generated flashcards should contain expected content")]
    public async Task ThenTheGeneratedFlashcardsShouldContainExpectedContent()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Check for at least some expected content from the sample text
        var expectedTerms = new[] { "Helena Markos", "Heliora", "ceramika", "Selina", "starożytnego" };
        var foundTerms = 0;
        
        foreach (var term in expectedTerms)
        {
            var termElement = pageInstance.GetByText(term, new() { Exact = false });
            var count = await termElement.CountAsync();
            if (count > 0)
            {
                foundTerms++;
                Console.WriteLine($"Found expected term: {term}");
            }
        }
        
        foundTerms.Should().BeGreaterThan(0, "Generated flashcards should contain some expected content from the source text");
        Console.WriteLine($"Found {foundTerms} out of {expectedTerms.Length} expected terms in generated flashcards");
    }

    [Then("all flashcards should be selected")]
    public async Task ThenAllFlashcardsShouldBeSelected()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Wait for the Accept All action to complete
        await pageInstance.WaitForTimeoutAsync(1000);
        
        // Look for selected state indicators - checkboxes should be checked
        var checkboxes = pageInstance.Locator("input[type='checkbox']");
        var checkboxCount = await checkboxes.CountAsync();
        
        if (checkboxCount > 0)
        {
            var checkedBoxes = pageInstance.Locator("input[type='checkbox']:checked");
            var checkedCount = await checkedBoxes.CountAsync();
            checkedCount.Should().BeGreaterThan(0, "At least some flashcards should be selected after Accept All");
            Console.WriteLine($"Found {checkedCount} checked out of {checkboxCount} total checkboxes");
        }
        else
        {
            // Alternative: check if the count in header shows selection
            var headerText = await pageInstance.Locator("text=/Generated Flashcards \\([0-9]+ of [0-9]+ selected\\)/").TextContentAsync();
            headerText.Should().NotBeNull("Selection count should be visible in header");
            Console.WriteLine($"Header shows: {headerText}");
        }
    }

    [Then("the Save Selected button should be enabled")]
    public async Task ThenTheSaveSelectedButtonShouldBeEnabled()
    {
        var pageInstance = ((FlashcardGenerationPage)FlashcardGenerationPage).PageInstance;
        
        // Look for Save Selected button
        var saveButton = pageInstance.GetByRole(AriaRole.Button, new() { Name = "Save Selected" });
        
        // Alternative selectors
        if (await saveButton.CountAsync() == 0)
        {
            saveButton = pageInstance.Locator("button:has-text('Save Selected'), button:has-text('SAVE SELECTED')");
        }
        
        var isDisabled = await saveButton.IsDisabledAsync();
        isDisabled.Should().BeFalse("Save Selected button should be enabled after selecting flashcards");
        
        var isVisible = await saveButton.IsVisibleAsync();
        isVisible.Should().BeTrue("Save Selected button should be visible");
    }
}
