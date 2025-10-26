# Functional Tests Architecture

This project follows SOLID principles and clean code practices for maintainable and scalable functional tests.

## Architecture Overview

### 📁 Project Structure

```
Fiszki.FunctionalTests/
├── Abstractions/          # Interfaces and contracts
│   ├── IPageFactory.cs    # Factory interface for page creation
│   └── IPageInterfaces.cs # Page-specific interfaces
├── Constants/             # Centralized constants
│   └── TestConstants.cs   # All magic strings, timeouts, selectors
├── PageObjects/           # Page Object Model implementation
│   ├── BasePage.cs        # Base functionality for all pages
│   ├── LoginPage.cs       # Login page implementation
│   ├── RegisterPage.cs    # Register page implementation
│   ├── HomePage.cs        # Home page implementation
│   └── NavBar.cs          # Navigation component
├── Services/              # Business logic and utilities
│   └── PageFactory.cs     # Creates page instances
└── Steps/                 # SpecFlow step definitions
    ├── Base/
    │   └── BaseSteps.cs   # Common step functionality
    ├── LoginSteps.cs      # Login-specific steps
    ├── RegisterSteps.cs   # Register-specific steps
    └── NavigationSteps.cs # Navigation-specific steps
```

## SOLID Principles Applied

### 🔹 Single Responsibility Principle (SRP)
- Each class has one reason to change
- `LoginPage` handles only login-related functionality
- `PageFactory` handles only page creation
- Step classes handle only their specific domain

### 🔹 Open/Closed Principle (OCP)
- Pages extend `BasePage` without modifying it
- New page types can be added by implementing interfaces
- Step classes extend `BaseSteps` for shared functionality

### 🔹 Liskov Substitution Principle (LSP)
- All page implementations can be used wherever their interfaces are expected
- `ILoginPage`, `IRegisterPage` can be substituted seamlessly

### 🔹 Interface Segregation Principle (ISP)
- Small, focused interfaces (`INavigable`, `IFormPage`)
- Pages only implement interfaces they need
- No forced dependencies on unused methods

### 🔹 Dependency Inversion Principle (DIP)
- Step classes depend on abstractions (`IPageFactory`)
- High-level modules don't depend on low-level modules
- Dependencies are injected, not created

## Key Features

### ✅ Clean Code Practices
- **No magic strings**: All selectors, messages, and routes in constants
- **No code duplication**: Shared logic in base classes
- **Meaningful names**: Clear, descriptive method and class names
- **Small methods**: Each method has a single purpose

### ✅ Maintainability
- **Centralized configuration**: All timeouts and selectors in one place
- **Lazy initialization**: Pages created only when needed
- **Error handling**: Proper null checks and exception handling
- **Consistent patterns**: All classes follow the same structure

### ✅ Testability
- **Interface-driven design**: Easy to mock and test
- **Separation of concerns**: Logic isolated in appropriate layers
- **Factory pattern**: Centralized object creation

## Usage Examples

### Creating a Page
```csharp
// Old way (violates DIP)
var loginPage = new LoginPage(page, baseUrl);

// New way (follows DIP)
var loginPage = pageFactory.CreatePage<LoginPage>();
```

### Step Implementation
```csharp
[Binding]
public class LoginSteps : BaseSteps
{
    private ILoginPage LoginPage => _loginPage ??= GetPage<LoginPage>();
    
    [Given("I am on the Login page")]
    public async Task GivenIAmOnLoginPage() => await LoginPage.NavigateAsync();
}
```

## Adding New Pages

1. Create interface in `IPageInterfaces.cs`
2. Implement page class inheriting from `BasePage`
3. Add to `PageFactory.cs`
4. Create step class inheriting from `BaseSteps`

## Constants Usage

Instead of magic strings:
```csharp
// ❌ Bad
await page.WaitForTimeoutAsync(1000);
await page.FillAsync("#emailInput", email);

// ✅ Good
await page.WaitForTimeoutAsync(TestConstants.Timeouts.DefaultWaitMs);
await page.FillAsync(TestConstants.Selectors.EmailInput, email);
```
