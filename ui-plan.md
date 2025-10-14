# UI Architecture for 10x-cards

## 1. UI Structure Overview

The application uses Blazor Server with MudBlazor components and secure JWT-based authentication. The `MainLayout` provides a sidebar (`NavMenu`) and a content area. Users authenticate via login/register views before accessing protected pages: Generate, My Flashcards, Study Session, and Account Settings. A global `ErrorBoundary` and `ToastService` handle errors and notifications.

## 2. View List

### 2.1 Login View
- Path: `/login`
- Main purpose: User authentication.
- Key information: Email and password fields, Login button, Forgot password link.
- Key components: `MudTextField`, `MudButton`, `MudForm`.
- Considerations: ARIA labels for fields and error messages; redirect to `/generate` on success; throttle login attempts; display validation and server errors.

### 2.2 Register View
- Path: `/register`
- Main purpose: New user account creation.
- Key information: Email, password, confirm password fields; register button.
- Key components: `MudTextField`, `MudForm`, `MudButton`.
- Considerations: DataAnnotations for password strength; client-side validation; show success toast and navigate to `/generate`.

### 2.3 Generate Flashcards View
- Path: `/generate`
- Main purpose: AI-based flashcard generation workflow.
- Key information: Text input area, Generate button, loading indicator, list of proposals.
- Key components: `MudTextField` (multiline), `MudButton`, `MudProgressCircular`, `ProposalList` (collection of `ProposalCard`), `MudDialog` for editing proposals.
- Considerations: Validate length (1 000–10 000 chars); ARIA roles on proposal actions; handle API errors (rate limit, timeout) via `ToastService`.

### 2.4 Review Proposals Dialog
- Path: modal on `/generate`
- Main purpose: Edit or accept individual AI flashcard suggestions before final save.
- Key information: Front/back fields, tags input, Accept, Reject, Edit buttons.
- Key components: `MudDialog`, `MudTextField`, `MudChipSet` for tags.
- Considerations: Focus trap in dialog; keyboard navigation; confirmation on bulk accept; validation on field edits.

### 2.5 My Flashcards View
- Path: `/flashcards`
- Main purpose: List, filter, and manage user flashcards.
- Key information: Paginated table of flashcards (front, back, tags, source, dates), filter/search bar.
- Key components: `MudTable` with server-side pagination, `MudTextField` for search, `MudSelect` for source filter, `MudPagination`, `MudDialog` for Create/Edit.
- Considerations: Secure filtering by user; DataAnnotations validation in dialogs; ARIA compliance in table headers.

### 2.6 Flashcard Create/Edit Dialog
- Path: modal on `/flashcards`
- Main purpose: Manual flashcard creation or editing.
- Key information: Front content, back content, tags, Save/Delete buttons.
- Key components: `MudDialog`, `MudForm`, `MudTextField`, `MudChipSet`, `MudButton`.
- Considerations: Confirm deletion; validate content length; display unsaved changes warning.

### 2.7 Study Session View
- Path: `/study`
- Main purpose: Display flashcards in SM-2 spaced-repetition session.
- Key information: Current flashcard front, reveal button, self-assessment buttons (e.g., Easy, Good, Hard).
- Key components: `MudCard`, `MudButtonGroup`, `MudProgressLinear` for session progress.
- Considerations: Keyboard shortcuts; announce card content to screen readers; secure session data.

### 2.8 Account Settings View
- Path: `/settings`
- Main purpose: Profile info and GDPR account deletion.
- Key information: User email, counters (generated, accepted), Delete account button.
- Key components: `MudText`, `MudButton`; confirmation dialog for deletion.
- Considerations: Confirm irreversible actions; re-authenticate before deletion; secure DELETE `/users/me` call.

### 2.9 Error / NotFound View
- Path: fallback route `*`
- Main purpose: Display 404 or unhandled exceptions.
- Key information: Error message, navigation links.
- Key components: `ErrorBoundary`, `MudAlert`, `MudButton`.
- Considerations: Clear messaging; link back to safe page.

## 3. User Journey Map

1. **Entry**: User lands on `/login` or `/register`.
2. **Authentication**: After login/register, user is navigated to `/generate`.
3. **Generate**: User pastes source text, clicks Generate.
4. **Review**: Proposals appear; user edits/accepts/rejects suggestions.
5. **Persist**: On accept, selected cards saved; toast confirms action.
6. **Manage**: User navigates to `/flashcards`, views, filters, edits, or deletes cards.
7. **Study**: User starts session at `/study`, reviews cards via SM-2 interaction.
8. **Settings**: User checks profile and may delete account at `/settings`.
9. **Logout**: Protected routes redirect to `/login` when token expires or on manual logout.

## 4. Layout and Navigation Structure

- **MainLayout**: Sidebar with NavMenu items: Generate, My Flashcards, Study Session, Settings; Top-right logout button.
- **Routing**: Use `<AuthorizeView>` and `[Authorize]` on pages; fallback route to NotFound.
- **Dialogs**: Overlaid via MudBlazor `DialogService` on Generate and Flashcards pages.

## 5. Key Components

1. **ProposalList / ProposalCard**: Displays AI suggestions with Accept/Edit/Reject actions.
2. **FlashcardTable**: Reusable component wrapping `MudTable` with paging, sorting, filtering.
3. **ErrorBoundaryWrapper**: Wraps `Router` to catch component errors.
4. **ToastService**: Centralized notification system.
5. **AuthStateProvider**: Custom provider storing JWT in ProtectedBrowserStorage and configuring `HttpClient`.
6. **ConfirmationDialog**: Generic dialog for delete and destructive actions.

