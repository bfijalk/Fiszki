using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class TestDelayHooks
{
    private static bool _isFirstScenario = true;

    [BeforeScenario(Order = int.MaxValue)] // Run this hook last
    public static async Task AddDelayBeforeScenario()
    {
        // Skip delay for the first scenario to avoid unnecessary wait at the start
        if (_isFirstScenario)
        {
            _isFirstScenario = false;
            Console.WriteLine("[Test Delay] Skipping delay for first scenario.");
            return;
        }

        Console.WriteLine("[Test Delay] Waiting 2 seconds before scenario to allow environment restoration...");
        await Task.Delay(TimeSpan.FromSeconds(2));
        Console.WriteLine("[Test Delay] Delay completed. Starting scenario.");
    }
}
