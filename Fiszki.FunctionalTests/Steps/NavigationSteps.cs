using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using FluentAssertions;
using TechTalk.SpecFlow;

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
}
