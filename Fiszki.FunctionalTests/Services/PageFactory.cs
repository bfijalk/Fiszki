using Microsoft.Playwright;
using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Support;

namespace Fiszki.FunctionalTests.Services;

public class PageFactory : IPageFactory
{
    private readonly ScenarioContext _scenarioContext;

    public PageFactory(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IPage Page => (IPage)_scenarioContext[TestContextKeys.Page];
    private string BaseUrl => (string)_scenarioContext[TestContextKeys.BaseUrl];

    public T CreatePage<T>() where T : class
    {
        return typeof(T).Name switch
        {
            nameof(LoginPage) => new LoginPage(Page, BaseUrl) as T,
            nameof(RegisterPage) => new RegisterPage(Page, BaseUrl) as T,
            nameof(HomePage) => new HomePage(Page, BaseUrl) as T,
            nameof(NavBar) => new NavBar(Page) as T,
            nameof(FlashcardGenerationPage) => new FlashcardGenerationPage(Page, BaseUrl) as T,
            nameof(FlashcardsPage) => new FlashcardsPage(Page, BaseUrl) as T,
            _ => throw new NotSupportedException($"Page type {typeof(T).Name} is not supported")
        } ?? throw new InvalidOperationException($"Failed to create page of type {typeof(T).Name}");
    }
}
