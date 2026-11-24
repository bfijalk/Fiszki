using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

/// <summary>
/// Steps for FlashcardArchiving.feature ("I learned it", the archived view, restoring and the learning
/// progress). The database is shared with other scenarios, so every assertion is relative to what the
/// scenario remembered before acting and never to absolute numbers.
/// </summary>
[Binding]
public class FlashcardArchivingSteps : BaseSteps
{
    private IFlashcardsPage? _flashcardsPage;
    private int _rememberedTotal;
    private int _rememberedArchived;
    private int _rememberedProgress;

    public FlashcardArchivingSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
    }

    private IFlashcardsPage FlashcardsPage => _flashcardsPage ??= GetPage<FlashcardsPage>();

    // ----- setup -----

    [When("I create the manual flashcard {string} with answer {string}")]
    public async Task WhenICreateTheManualFlashcard(string question, string answer)
    {
        await FlashcardsPage.ClickAddManualCardAsync();
        await FlashcardsPage.EnterQuestionAsync(question);
        await FlashcardsPage.EnterAnswerAsync(answer);
        await FlashcardsPage.ClickCreateCardAsync();
        (await FlashcardsPage.IsFlashcardVisibleAsync(question)).Should().BeTrue($"the new flashcard '{question}' is shown");
    }

    [When("I remember the flashcard counters and the learning progress")]
    public async Task WhenIRememberTheCountersAndProgress()
    {
        _rememberedTotal = await FlashcardsPage.GetTotalFlashcardCountAsync();
        _rememberedArchived = await FlashcardsPage.GetArchivedCountAsync();
        _rememberedProgress = await FlashcardsPage.GetLearningProgressAsync();
    }

    // ----- buttons -----

    [Then("the flashcard {string} should have the I learned it button")]
    public async Task ThenTheFlashcardShouldHaveTheArchiveButton(string cardText)
    {
        (await FlashcardsPage.HasArchiveButtonAsync(cardText)).Should().BeTrue($"'{cardText}' is active");
    }

    [Then("the flashcard {string} should not have the I learned it button")]
    public async Task ThenTheFlashcardShouldNotHaveTheArchiveButton(string cardText)
    {
        (await FlashcardsPage.HasArchiveButtonAsync(cardText)).Should().BeFalse($"'{cardText}' is archived");
    }

    [Then("the flashcard {string} should offer Restore and Delete")]
    public async Task ThenTheFlashcardShouldOfferRestoreAndDelete(string cardText)
    {
        (await FlashcardsPage.HasRestoreButtonAsync(cardText)).Should().BeTrue("an archived flashcard can be restored");
        (await FlashcardsPage.HasDeleteButtonAsync(cardText)).Should().BeTrue("an archived flashcard can be deleted");
    }

    // ----- confirmation dialog -----

    [When("I click I learned it on the flashcard {string}")]
    public async Task WhenIClickIlearnedItOnTheFlashcard(string cardText)
    {
        await FlashcardsPage.ClickArchiveCardAsync(cardText);
    }

    [Then("I should see the archive confirmation modal showing {string}")]
    public async Task ThenIShouldSeeTheArchiveConfirmationModal(string cardText)
    {
        (await FlashcardsPage.IsArchiveModalVisibleAsync()).Should().BeTrue();
        (await FlashcardsPage.GetArchiveModalTextAsync()).Should().Contain(cardText);
    }

    [When("I confirm the archiving")]
    public async Task WhenIConfirmTheArchiving() => await FlashcardsPage.ConfirmArchiveAsync();

    [When("I cancel the archiving")]
    public async Task WhenICancelTheArchiving() => await FlashcardsPage.CancelArchiveAsync();

    [Then("the archive confirmation modal should be closed")]
    public async Task ThenTheArchiveConfirmationModalShouldBeClosed()
    {
        (await FlashcardsPage.IsArchiveModalVisibleAsync()).Should().BeFalse();
    }

    // ----- views and restore -----

    [When("I switch to the archived view")]
    public async Task WhenISwitchToTheArchivedView() => await FlashcardsPage.SwitchToArchivedViewAsync();

    [When("I switch to the active view")]
    public async Task WhenISwitchToTheActiveView() => await FlashcardsPage.SwitchToActiveViewAsync();

    [When("I restore the flashcard {string}")]
    public async Task WhenIRestoreTheFlashcard(string cardText) => await FlashcardsPage.RestoreCardAsync(cardText);

    [Then("the archived view should say that no flashcards are archived")]
    public async Task ThenTheArchivedViewShouldSayNothingIsArchived()
    {
        (await FlashcardsPage.IsViewEmptyMessageVisibleAsync()).Should().BeTrue();
    }

    // ----- counters and learning progress -----

    [Then("the learning progress should be {int} percent")]
    public async Task ThenTheLearningProgressShouldBePercent(int percent)
    {
        (await FlashcardsPage.GetLearningProgressAsync()).Should().Be(percent);
    }

    [Then("the total flashcards counter should be {int}")]
    public async Task ThenTheTotalShouldBe(int total)
    {
        (await FlashcardsPage.GetTotalFlashcardCountAsync()).Should().Be(total);
    }

    [Then("the list should say that there are no flashcards to show")]
    public async Task ThenTheListShouldSayThereAreNoFlashcards()
    {
        (await FlashcardsPage.IsViewEmptyMessageVisibleAsync()).Should().BeTrue();
    }

    [Then("the learning progress should be shown")]
    public async Task ThenTheLearningProgressShouldBeShown()
    {
        (await FlashcardsPage.IsLearningProgressVisibleAsync()).Should().BeTrue();
    }

    [Then("the learning progress should be higher than before")]
    public async Task ThenTheLearningProgressShouldBeHigher()
    {
        (await FlashcardsPage.GetLearningProgressAsync()).Should().BeGreaterThan(_rememberedProgress);
    }

    [Then("the learning progress should be lower than before")]
    public async Task ThenTheLearningProgressShouldBeLower()
    {
        (await FlashcardsPage.GetLearningProgressAsync()).Should().BeLessThan(_rememberedProgress);
    }

    [Then("the learning progress should be the same as before")]
    public async Task ThenTheLearningProgressShouldBeTheSame()
    {
        (await FlashcardsPage.GetLearningProgressAsync()).Should().Be(_rememberedProgress);
    }

    [Then("the archived count should be {int} higher than before")]
    public async Task ThenTheArchivedCountShouldBeHigher(int difference)
    {
        (await FlashcardsPage.GetArchivedCountAsync()).Should().Be(_rememberedArchived + difference);
    }

    [Then("the archived count should be {int} lower than before")]
    public async Task ThenTheArchivedCountShouldBeLower(int difference)
    {
        (await FlashcardsPage.GetArchivedCountAsync()).Should().Be(_rememberedArchived - difference);
    }

    [Then("the archived count should be the same as before")]
    public async Task ThenTheArchivedCountShouldBeTheSame()
    {
        (await FlashcardsPage.GetArchivedCountAsync()).Should().Be(_rememberedArchived);
    }

    [Then("the total flashcards counter should be the same as before")]
    public async Task ThenTheTotalShouldBeTheSame()
    {
        (await FlashcardsPage.GetTotalFlashcardCountAsync()).Should().Be(_rememberedTotal);
    }

    [Then("the total flashcards counter should be {int} lower than before")]
    public async Task ThenTheTotalShouldBeLower(int difference)
    {
        (await FlashcardsPage.GetTotalFlashcardCountAsync()).Should().Be(_rememberedTotal - difference);
    }

    [Then("the learning progress should match the archived and total flashcards")]
    public async Task ThenTheLearningProgressShouldMatch()
    {
        var total = await FlashcardsPage.GetTotalFlashcardCountAsync();
        var archived = await FlashcardsPage.GetArchivedCountAsync();
        var progress = await FlashcardsPage.GetLearningProgressAsync();
        archived.Should().BeLessThanOrEqualTo(total);
        progress.Should().Be(total == 0 ? 0 : archived * 100 / total, $"{archived} of {total} are archived");
    }
}
