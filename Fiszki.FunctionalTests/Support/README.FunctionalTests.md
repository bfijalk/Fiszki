# Fiszki.FunctionalTests

Simple black-box UI functional tests using SpecFlow (Gherkin) + Playwright.

## What is covered
- Login validation error when credentials missing
- Login invalid credentials message
- Register password mismatch message
- Navigation shows Login link when unauthenticated

(No successful auth scenarios – they would require DB seed.)

## Running
Ensure the web app is running on the expected URL:

```bash
# start app in another terminal
dotnet run --project Fiszki --urls http://localhost:5290
```

Install Playwright browsers if first time:
```bash
dotnet build Fiszki.FunctionalTests/Fiszki.FunctionalTests.csproj
playwright install
# or: dotnet playwright install (if global tool installed)
```

Execute tests:
```bash
dotnet test Fiszki.FunctionalTests/Fiszki.FunctionalTests.csproj -c Debug
```

## Notes
- Tests interact only via HTTP and DOM.
- Keep selectors simple & resilient.
- Extend with additional features (e.g., logout, protected pages) once test data strategy is defined.

