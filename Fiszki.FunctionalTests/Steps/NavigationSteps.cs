using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using FluentAssertions;
using TechTalk.SpecFlow;
using Microsoft.Playwright;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class NavigationSteps : BaseSteps
{
    private HomePage? _homePage;
    private NavBar? _navBar;

    public NavigationSteps(ScenarioContext scenarioContext) : base(scenarioContext) { }

    private HomePage HomePage => _homePage ??= GetPage<HomePage>();
    private NavBar NavBar => _navBar ??= GetPage<NavBar>();

    [Given("I am on the Home page")]
    [Given("I am on the home page")]
    public async Task GivenIAmOnHomePage()
    {
        await HomePage.NavigateAsync();
    }

    [Then("I should see a nav link \"(.*)\"")]
    public async Task ThenIShouldSeeNavLink(string linkText)
    {
        var link = NavBar.GetLink(linkText);
        await link.WaitForAsync();
        var text = await link.InnerTextAsync();
        text.Should().Contain(linkText);
    }

    [When("I click \"(.*)\" button")]
    [Given("I click \"(.*)\" button")]
    public async Task WhenIClickButton(string buttonText)
    {
        // Get the page instance from HomePage 
        var page = HomePage.PageInstance;
        var button = page.GetByRole(AriaRole.Button, new() { Name = buttonText });
        await button.ClickAsync();
        await page.WaitForTimeoutAsync(1000); // Wait for any navigation
    }

    [When("I click \"(.*)\" link")]
    [Given("I click \"(.*)\" link")]
    public async Task WhenIClickLink(string linkText)
    {
        var page = HomePage.PageInstance;
        var link = page.GetByRole(AriaRole.Link, new() { Name = linkText });
        await link.ClickAsync();
        await page.WaitForTimeoutAsync(1000); // Wait for navigation
    }
}
