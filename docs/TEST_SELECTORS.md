# Test Selectors Documentation

This document describes all `data-testid` attributes added to the Fiszki application for automated UI testing with Playwright, Cypress, or similar frameworks.

## Overview

All interactive elements in the application now have `data-testid` attributes that make them easy to locate in automated tests. These selectors are stable and won't change with UI styling updates.

## How to Use

In Playwright:
```csharp
await Page.GetByTestId("login-button").ClickAsync();
```

In JavaScript/TypeScript (Playwright or Cypress):
```javascript
await page.getByTestId('login-button').click();
// or with Cypress:
cy.get('[data-testid="login-button"]').click();
```

---

## Login Page (`/login`)

### Form Fields
- `email-input` - Email input field
- `password-input` - Password input field

### Buttons
- `login-button` - Main login submit button
- `create-account-button` - Navigate to registration page

### Alerts
- `login-error-alert` - Error message display

---

## Register Page (`/register`)

### Form Fields
- `email-input` - Email input field
- `password-input` - Password input field
- `password-confirm-input` - Password confirmation field

### Buttons
- `register-button` - Main registration submit button
- `back-to-login-button` - Navigate back to login page

### Messages
- `password-mismatch-message` - Password mismatch warning
- `register-error-alert` - Registration error message
- `register-success-alert` - Registration success message

---

## Home Page (`/`)

### Buttons
- `get-started-signin-button` - Primary call-to-action button (hero section)
- `start-learning-button` - Secondary call-to-action button (bottom section)

---

## Navigation Menu

### Navigation Links
- `navbar-brand` - Fiszki logo/brand link
- `nav-home` - Home page link
- `nav-how-to-start` - How to Start page link
- `nav-examples` - Examples page link
- `nav-about-technicalities` - About Technicalities page link
- `nav-about` - About Me page link

### Authenticated User Links
- `nav-flashcards` - Flashcards page link (only visible when logged in)
- `nav-generate` - Generate page link (only visible when logged in)
- `nav-logout` - Logout button (only visible when logged in)

### Unauthenticated User Links
- `nav-login` - Login page link (only visible when logged out)

---

## Flashcards Page (`/flashcards`)

### Page States
- `flashcards-loading-spinner` - Loading spinner
- `flashcards-error-alert` - Error message
- `flashcards-empty-state` - Empty state container (no flashcards)

### Empty State Buttons
- `generate-with-ai-button` - Navigate to Generate page
- `create-manually-button` - Open manual creation modal

### Statistics Cards
- `total-cards-stat` - Total flashcards statistics card
- `total-count` - Total count number
- `ai-cards-stat` - AI-generated flashcards statistics card
- `ai-count` - AI-generated count number
- `manual-cards-stat` - Manual flashcards statistics card
- `manual-count` - Manual count number

### Action Buttons
- `generate-more-cards-button` - Navigate to Generate page
- `add-manual-card-button` - Open manual creation modal
- `toggle-view-mode-button` - Switch between card and list view

### View Containers
- `flashcards-card-view` - Card view container
- `flashcards-list-view` - List view container
- `flashcard-item` - Individual flashcard item

### Manual Card Creation Modal
- `create-manual-card-modal` - Modal container
- `question-front-input` - Question/front content textarea
- `answer-back-input` - Answer/back content textarea
- `tags-input` - Tags input field
- `create-card-button` - Submit/create button
- `cancel-create-button` - Cancel button
- `close-create-modal-button` - Close (X) button
- `create-error-alert` - Error message in modal

### Delete Confirmation Modal
- `delete-confirmation-modal` - Modal container
- `confirm-delete-button` - Confirm delete button
- `cancel-delete-button` - Cancel button (X icon)
- `cancel-delete-modal-button` - Cancel button (text)

---

## Generate Page (`/generate`)

### Source Input Form
- `source-input-form` - Form container
- `source-text-input` - Source text textarea
- `language-select` - Language selection dropdown
- `max-cards-input` - Maximum cards number input
- `generate-flashcards-button` - Generate button

### Proposals Section
- `proposals-section` - Proposals container
- `proposals-header` - Section header with count
- `accept-all-button` - Accept all proposals button
- `reject-all-button` - Reject all proposals button
- `save-selected-button` - Save selected proposals button

### Individual Proposals
- `proposal-item` - Individual proposal card
- `proposal-front` - Front/question content
- `proposal-back` - Back/answer content
- `proposal-example` - Example text (if available)
- `edit-proposal-button` - Edit proposal button
- `reject-proposal-button` - Reject proposal button
- `undo-reject-button` - Undo rejection button

---

## Best Practices for Testers

1. **Always use `data-testid` selectors** instead of CSS classes or text content, as they are more stable.

2. **Wait for elements to be visible** before interacting with them:
   ```csharp
   await Page.GetByTestId("login-button").WaitForAsync(new() { State = WaitForSelectorState.Visible });
   await Page.GetByTestId("login-button").ClickAsync();
   ```

3. **Check button states** before clicking:
   ```csharp
   var isDisabled = await Page.GetByTestId("login-button").IsDisabledAsync();
   ```

4. **Use specific selectors** for better test reliability. For example, use `data-testid="login-button"` instead of searching by button text.

5. **Check for loading states** and wait for them to complete:
   ```csharp
   await Page.GetByTestId("flashcards-loading-spinner").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
   ```

---

## Example Test Scenarios

### Login Flow
```csharp
// Navigate to login
await Page.GetByTestId("nav-login").ClickAsync();

// Fill in credentials
await Page.GetByTestId("email-input").FillAsync("user@example.com");
await Page.GetByTestId("password-input").FillAsync("password123");

// Submit
await Page.GetByTestId("login-button").ClickAsync();

// Verify redirect
await Page.WaitForURLAsync("**/flashcards");
```

### Create Manual Flashcard
```csharp
// Navigate to flashcards
await Page.GetByTestId("nav-flashcards").ClickAsync();

// Open creation modal
await Page.GetByTestId("add-manual-card-button").ClickAsync();

// Fill in flashcard
await Page.GetByTestId("question-front-input").FillAsync("What is 2+2?");
await Page.GetByTestId("answer-back-input").FillAsync("4");
await Page.GetByTestId("tags-input").FillAsync("math, basic");

// Submit
await Page.GetByTestId("create-card-button").ClickAsync();

// Verify creation
await Expect(Page.GetByTestId("create-manual-card-modal")).ToBeHiddenAsync();
```

### Generate Flashcards with AI
```csharp
// Navigate to generate page
await Page.GetByTestId("nav-generate").ClickAsync();

// Enter source text
await Page.GetByTestId("source-text-input").FillAsync("Long educational text here...");

// Set parameters
await Page.GetByTestId("language-select").SelectOptionAsync("en");
await Page.GetByTestId("max-cards-input").FillAsync("10");

// Generate
await Page.GetByTestId("generate-flashcards-button").ClickAsync();

// Wait for proposals
await Page.GetByTestId("proposals-section").WaitForAsync(new() { State = WaitForSelectorState.Visible });

// Accept all and save
await Page.GetByTestId("accept-all-button").ClickAsync();
await Page.GetByTestId("save-selected-button").ClickAsync();
```

---

## Notes

- All selectors are case-sensitive
- Selectors use kebab-case naming convention (e.g., `login-button`, not `loginButton`)
- No application logic was changed - only `data-testid` attributes were added
- These selectors are designed to be stable across UI changes

