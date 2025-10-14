# UI Implementation Plan for 10x-cards MVP

This plan summarizes decisions made so far for the Blazor Server UI architecture.

## 1. Layout & Navigation
- Use built-in Blazor routing and `AuthenticationStateProvider`.
- Two top-level views under `MainLayout.razor`:
  - `/generate` for AI flashcard generation workflow.
  - `/flashcards` for manual CRUD and list of user flashcards.
- Protect routes using built-in `[Authorize]` and redirect unauthenticated users to login.

## 2. UI Component Library
- Adopt **MudBlazor** for all forms, tables, dialogs, notifications, and navigation components.

## 3. Authentication & Token Storage
- Implement a custom `AuthenticationStateProvider` that stores JWT in `ProtectedBrowserStorage`.
- Configure `HttpClient` in `Program.cs` with an `AuthorizationMessageHandler` to attach bearer tokens automatically.

## 4. Accessibility
- Assign ARIA labels and roles to all interactive elements (buttons, dialogs) in Razor components.
- Plan validation of accessibility using Playwright tests.

## 5. Error Handling
- Create a global `ErrorBoundary` component wrapping `App.razor`.
- Integrate a `ToastService` (MudBlazor) for displaying API and runtime errors as pop-up notifications.

## 6. State Management
- For now, skip a dedicated scoped `AiSessionService`; rely on local component state within the Generate page.

## 7. Validation
- Mirror server-side constraints using DataAnnotations on models:
  - `StringLength` and `Required` attributes for front/back content.
  - Tag count and length limits.
- Use `<EditForm>` with `<ValidationSummary>` to prevent invalid submissions.

## 8. Pagination & Performance
- Use MudBlazor’s table or pagination component.
- Implement server-side paging by sending `page` and `pageSize` query parameters to `GET /flashcards`.
- Cache fetched pages in a scoped service if needed to reduce API calls.

