using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Support;
using Microsoft.Playwright;
using FluentAssertions;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class InfrastructureSteps
{
    private readonly ScenarioContext _scenario;
    public InfrastructureSteps(ScenarioContext scenario) => _scenario = scenario;

    private IPage Page => (IPage)_scenario[TestContextKeys.Page];
    private string BaseUrl => (string)_scenario[TestContextKeys.BaseUrl];

    [Given("the application is running")]
    public async Task GivenTheApplicationIsRunning()
    {
        await EnsureHomePageAsync();
    }

    private async Task EnsureHomePageAsync()
    {
        var attempts = 0;
        Exception? lastEx = null;
        while (attempts < 8)
        {
            attempts++;
            try
            {
                var response = await Page.GotoAsync(BaseUrl + "/", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
                // Some environments may return 304 / cached etc.
                response.Should().NotBeNull("navigation should return a response");
                var status = response!.Status;
                if (status >= 200 && status < 400)
                {
                    var content = await Page.ContentAsync();
                    if (content.Contains("Hello, world!"))
                    {
                        return; // success
                    }
                    lastEx = new InvalidOperationException("Home page loaded but expected marker text not found.");
                }
                else
                {
                    lastEx = new InvalidOperationException($"Unexpected HTTP status {status} when loading home page.");
                }
            }
            catch (Exception ex)
            {
                lastEx = ex;
            }
            await Task.Delay(500); // brief backoff before retry
        }
        throw new InvalidOperationException($"Failed to confirm application running at '{BaseUrl}' after {attempts} attempts. Last error: {lastEx?.Message}");
    }
}
