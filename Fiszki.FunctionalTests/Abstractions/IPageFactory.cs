using Microsoft.Playwright;

namespace Fiszki.FunctionalTests.Abstractions;

public interface IPageFactory
{
    T CreatePage<T>() where T : class;
}
