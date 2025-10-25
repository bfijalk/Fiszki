using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class RegisterSteps : BaseSteps
{
    private IRegisterPage? _registerPage;

    public RegisterSteps(ScenarioContext scenarioContext) : base(scenarioContext) { }

    private IRegisterPage RegisterPage => _registerPage ??= GetPage<RegisterPage>();

    [Given("I am on the Register page")]
    public async Task GivenIAmOnRegisterPage()
    {
        await RegisterPage.NavigateAsync();
    }

    [When("I enter register email \"(.*)\"")]
    [Given("I enter register email \"(.*)\"")]
    public async Task WhenIEnterRegisterEmail(string email)
    {
        await RegisterPage.EnterEmailAsync(email);
    }

    [When("I enter register password \"(.*)\"")]
    [Given("I enter register password \"(.*)\"")]
    public async Task WhenIEnterRegisterPassword(string password)
    {
        await RegisterPage.EnterPasswordAsync(password);
    }

    [When("I enter register confirm password \"(.*)\"")]
    [Given("I enter register confirm password \"(.*)\"")]
    public async Task WhenIEnterRegisterConfirmPassword(string confirmPassword)
    {
        await RegisterPage.EnterConfirmPasswordAsync(confirmPassword);
    }

    [Then("I should see password mismatch message")]
    public async Task ThenIShouldSeePasswordMismatchMessage()
    {
        await Task.Delay(TestConstants.Timeouts.PasswordMismatchWaitMs);
        var messageText = await RegisterPage.PasswordMismatchMessage.InnerTextAsync();
        messageText.Should().Contain(TestConstants.Messages.PasswordMismatch);
    }
}
