using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Services;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps.Base;

public abstract class BaseSteps
{
    protected readonly IPageFactory PageFactory;

    protected BaseSteps(ScenarioContext scenarioContext)
    {
        PageFactory = new PageFactory(scenarioContext);
    }

    protected T GetPage<T>() where T : class => PageFactory.CreatePage<T>();
}
