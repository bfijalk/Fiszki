# Conversation Summary for UI Architecture Planning

<conversation_summary>
<decisions>
1. Use built-in Blazor routing and `AuthenticationStateProvider` for navigation and route protection.
2. Adopt **MudBlazor** as the primary UI component library for forms, tables, dialogs, and notifications.
3. Assign ARIA labels and roles in Razor components; plan to validate accessibility with Playwright tests.
4. Store JWT tokens in `ProtectedBrowserStorage` via a custom `AuthenticationStateProvider`.
5. Skip creation of a dedicated `AiSessionService`; rely on local component state in the Generate page for AI proposals.
6. Create a global `ErrorBoundary` component and integrate a MudBlazor `ToastService` for error handling.
7. Mirror server-side validation constraints using DataAnnotations in models and `<EditForm>` for client-side validation.
8. Implement server-side pagination in the Flashcards list by passing `page` and `pageSize` parameters.
</decisions>
<matched_recommendations>
1. Use built-in Blazor systems for route management and authentication state.
2. Adopt MudBlazor for rapid development of UI components.
3. Ensure ARIA labels/roles for interactive elements and validate accessibility.
4. Persist JWT in ProtectedBrowserStorage and configure HttpClient with an Authorization handler.
5. Skip scoped AI session service; manage proposals in component state.
6. Implement a global ErrorBoundary and ToastService for unified error presentation.
7. Apply DataAnnotations and `<EditForm>` validation to prevent invalid submissions.
8. Use server-side pagination parameters to optimize Flashcards retrieval.
</matched_recommendations>
<ui_architecture_planning_summary>
The UI will be built on Blazor Server using MudBlazor as the component library. Routing and authentication are handled via Blazor’s `AuthenticationStateProvider`, storing tokens securely in `ProtectedBrowserStorage`. The main layout (`MainLayout.razor`) will host a sidebar navigation (`NavMenu`) and content area, with two primary pages: 

- **Generate (/generate)**: Paste text, call POST `/flashcards/ai/generate`, display proposals in a MudBlazor table or card list, allow Accept/Edit/Reject per proposal, then call POST `/flashcards/ai/accept`.
- **My Flashcards (/flashcards)**: List user flashcards with server-side pagination (page, pageSize), filtering, and sorting via GET `/flashcards`; allow manual Create, Edit, Delete via MudBlazor dialogs/forms.

State for AI proposals will be kept locally in the Generate component. Error handling across the app will use a global `ErrorBoundary` wrapper and MudBlazor’s `ToastService` for notifications. Accessibility is enforced by ARIA roles/labels; validation is implemented via DataAnnotations on view models and `<EditForm>` components. Security relies on JWT-based authorization, with all API calls routed through an `HttpClient` configured to include the bearer token.

This approach ensures a clear separation between major user flows, consistent styling, and adherence to MVP performance, accessibility, and security requirements.
</ui_architecture_planning_summary>
<unresolved_issues>
- Detailed component hierarchy and interaction patterns within the Generate page (e.g., ProposalList, ProposalEditor) need to be defined.
- Specific responsive layout breakpoints and styling guidelines (mobile vs. desktop) remain to be documented.
- Implementation details for `HttpClient` registration and `AuthorizationMessageHandler` in `Program.cs` must be finalized.
- Caching strategy beyond pagination (e.g., in-memory reuse of fetched pages) to reduce API calls requires further consideration.
</unresolved_issues>
</conversation_summary>
