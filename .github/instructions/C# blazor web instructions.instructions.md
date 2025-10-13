applyTo: '**'
---

# GitHub Copilot Project Instructions – Blazor (.NET 8) Fiszki

These instructions guide GitHub Copilot when generating or reviewing code for this repository. They define architecture context, coding standards, patterns, and constraints. Treat them as authoritative unless a newer requirement is added in a PR description.

## Role & Perspective
Act as a senior Blazor & .NET engineer. Produce idiomatic, performant, testable code for a .NET 8 Blazor Server application (interactive server render mode). Favor clarity and maintainability over premature micro‑optimizations. Always assume nullable reference types are enabled (implicit in .NET 8 SDK) and respect analyzers when suggesting code.

## Project Overview
- Hosting model: Blazor Server (.AddInteractiveServerComponents / AddInteractiveServerRenderMode()).
- Entry point: `Program.cs` using minimal hosting; no explicit Startup class.
- UI stack: Razor Components in `Components/` (Layouts, Pages) plus `wwwroot` static assets.
- Goal (inferred): Flashcard ("Fiszki") style learning app (expand with domain objects when introduced).

## Environment & Tooling
- Visual Studio / `dotnet` CLI compile, run, debug, test.
- GitHub Copilot assists authoring/refactoring inside editor; do NOT assume use of Cursor.
- When suggesting build / run steps, prefer CLI: `dotnet restore`, `dotnet build`, `dotnet watch`, `dotnet test`.
- Keep suggestions platform‑agnostic (macOS primary, but cross‑platform safe).

## Architectural & Code Organization Guidelines
1. Razor Components
	- Keep components small; extract reusable UI into `/Components` with PascalCase names.
	- Complex logic: move into injected services or partial class code‑behind (`Component.razor.cs`).
	- Use `@code` blocks only for lightweight logic < ~40 lines; otherwise prefer code‑behind.
2. Services / DI
	- Register services in `Program.cs`. Use interfaces (prefixed with I) for abstractions.
	- Favor constructor injection in code‑behind / services; minimal use of `@inject` in Razor (limit to view concerns).
3. Separation of Concerns
	- UI rendering (Razor) vs business logic (services) vs persistence (future: EF Core / repositories).
4. Async
	- Use `async`/`await` pervasively for I/O or long‑running operations. Avoid `.Result` or `.Wait()`.
	- When invoking multiple independent async operations, use `Task.WhenAll`.
5. Cancellation
	- Accept `CancellationToken` in service methods that may be long‑running or remote.
6. Configuration
	- Access settings via `IOptions<T>` / `IOptionsSnapshot<T>` for reloadable config.
7. Logging
	- Inject `ILogger<T>`; log at appropriate levels (Trace/Debug/Information/Warning/Error/Critical). Do not swallow exceptions silently.

## Naming Conventions
- PascalCase: public types, methods, properties, events, Razor component filenames.
- camelCase: local variables, parameters.
- _camelCase (leading underscore): private fields (backing fields) if needed.
- Interfaces: prefixed with I (e.g., `IFlashcardService`).
- Async methods: suffix `Async` (except event handlers implemented from framework signatures).

## Razor & Blazor Best Practices
- Lifecycle: override `OnInitialized{Async}`, `OnParametersSet{Async}`, `OnAfterRender{Async}` appropriately; avoid performing heavy work in sync variants that can block the circuit.
- State updates: after async operations that modify bound data, rely on automatic re-render; only call `StateHasChanged()` for external events / timers.
- Conditional rendering: use `if/else` blocks or `@switch`; avoid deeply nested ternaries for readability.
- Parameters: mark `[Parameter]` and `[EditorRequired]` where mandatory. Use `[CascadingParameter]` sparingly.
- Event handling: prefer `EventCallback` / `EventCallback<T>` for parent notification; avoid passing `Action` unless synchronous and cheap.
- Prevent unnecessary renders: override `ShouldRender()` when you can cheaply short‑circuit (rare; measure first).
- Forms: use `<EditForm>` with DataAnnotations or FluentValidation integration (future addition). Provide `ValidationMessage` components.

## Data Binding & Performance
- Use `@bind-Value` with explicit `Value` and `ValueChanged` for advanced patterns (two‑way binding with transformation).
- Avoid binding to complex expressions; bind to simple properties.
- For large lists, consider virtualization (`<Virtualize>` component) when list count may exceed a few hundred items.

## Error Handling
- Wrap external service / API calls in try/catch; log errors. Present user-friendly messages (never raw exception text).
- Use `ErrorBoundary` for component trees susceptible to intermittent failures; provide `OnError` logging.
- Do not catch broad `Exception` unless rethrowing or translating; prefer specific exception types.

## Validation
- Prefer DataAnnotations for simple forms (e.g., `[Required]`, `[StringLength]`).
- For complex or cross-field rules, plan for FluentValidation integration (add per future requirement) and centralize validators.

## Caching & State Management
- Blazor Server: ephemeral per user circuit. Use scoped services to hold per‑session state; avoid static mutable state.
- In‑memory caching: `IMemoryCache` for expensive lookups; implement sensible expiration.
- Consider distributed cache (Redis) when scaling across servers (future feature flag; do not add dependency prematurely).
- Avoid over-caching small, cheap queries.

## Future Persistence (Placeholder Guidance)
- When EF Core introduced: use async LINQ operators; no synchronous `ToList()` on large sets. Enable batching when feasible.
- Apply migrations via CLI (`dotnet ef migrations add <Name>`). Keep migration names descriptive.

## Security
- Always enforce HTTPS; already using `app.UseHttpsRedirection()`.
- When authentication added: use ASP.NET Core Identity or JWT bearer. Decorate components requiring auth with `<AuthorizeView>` or `[Authorize]` attributes on endpoints.
- Protect forms from forgery (antiforgery already enabled). Do not disable unless justified.
- Never log secrets or PII.

## Configuration & Secrets
- Use `appsettings.json` + environment overrides. Inject config sections into POCO option classes.
- Do not hardcode environment-specific values in code; use `IConfiguration`.
- Keep secrets out of repo (User Secrets / environment variables).

## Accessibility (a11y)
- Provide semantic HTML structure (headings in order, lists, labels tied to inputs via `for` / `id`).
- Use `aria-*` attributes only when native semantics insufficient.
- Ensure color contrast meets WCAG AA; avoid conveying meaning by color alone.

## Internationalization (i18n) (Planned)
- Prepare components to externalize user‑facing strings; avoid concatenating raw localized strings with markup.

## Logging & Telemetry
- Use structured logging: log values as parameters (`logger.LogInformation("Loaded {Count} cards", count);`).
- Avoid logging inside tight loops excessively.

## Testing Strategy
- Unit tests: xUnit (preferred) or MSTest; naming pattern `ClassNameTests` with method names `MethodName_ShouldExpectedBehavior_WhenCondition`.
- Component tests: Use `bUnit` (add when first testable component logic emerges).
- Mocking: `Moq` or `NSubstitute` (choose one; default recommendation: Moq).
- Avoid testing trivial property getters/setters; focus on logic/branch coverage.

## Performance Guidelines
- Minimize allocations in render paths (avoid LINQ in `@foreach` for hot lists; precompute in code-behind).
- Defer heavy async loads with `OnAfterRenderAsync(firstRender)` pattern.
- Use `CancellationToken` to abort obsolete requests (e.g., search-as-you-type scenario).
- Avoid large object graphs in circuit state; consider pagination.

## Async & Concurrency
- Always return `Task` / `Task<T>` for potentially asynchronous operations.
- Avoid `async void` except for event handlers.
- For fire-and-forget server work, capture and log exceptions (`_ = Task.Run(async () => { try { ... } catch (Exception ex) { logger.LogError(ex, ...); } });`).

## Code Style & Quality
- Enable analyzers and warnings; fix or suppress with justification comments (`// Suppress: reason`).
- Use expression-bodied members for concise, single-expression methods/properties.
- Leverage pattern matching (`switch expressions`, `is not null`).
- Prefer `readonly record struct` / `record` for immutable DTO-like types.

## Dependency Management
- Keep package additions minimal; justify in PR description (purpose, size, license). Avoid unnecessary UI libs.
- Prefer framework capabilities (built-in DI, logging, configuration) before adding libraries.

## Git / PR Conventions
- Commit messages: Conventional style (feat:, fix:, refactor:, test:, docs:, build:, chore:). Example: `feat(flashcards): add basic card entity`.
- Small, focused PRs (< 300 lines delta preferred) with description of rationale and any follow-up tasks.
- Include tests when altering behavior.

## Documentation
- Public APIs and services: XML doc comments for non-trivial logic.
- Razor components: brief comment at top if behavior is not self-evident.
- Keep README (add later) updated with new setup steps when dependencies/services added.

## Copilot Prompting Guidance (Meta)
When requesting Copilot completions:
- Provide the target file or component intent.
- Indicate whether creating new component, service, or test.
- Specify performance or accessibility concerns up front.
Copilot responses must:
- Avoid speculative dependencies not yet in the project.
- Avoid deprecated APIs.
- Be consistent with the rules herein.

## Anti-Patterns to Avoid
- Blocking calls in async methods (`.Result`, `.Wait()`).
- Storing per-user mutable state in static fields.
- Large monolithic Razor components (>500 lines) – refactor.
- Catch-all exception handlers that swallow errors.
- Overuse of `dynamic` or reflection for simple tasks.

## Acceptance Checklist (Use When Adding Features)
1. Adheres to naming & style guidelines.
2. Component small & cohesive; heavy logic extracted.
3. Async patterns correct; no deadlocks / blocking.
4. Logging present for failure paths.
5. Validation implemented if user input accepted.
6. Tests added/updated (logic or UI with bUnit when present).
7. No unnecessary dependencies.
8. Accessible markup (labels, semantic elements).

## Future Enhancements (Backlog Notes – Do Not Implement Automatically)
- Introduce EF Core persistence layer (Flashcards, Decks, Tags).
- Add authentication/authorization.
- Add bUnit test project.
- Add FluentValidation integration.
- Add OpenAPI/Swagger for backend API endpoints if/when APIs exposed.

---
If a new requirement conflicts with the above, the most recent explicit requirement in a PR or issue description wins. Keep suggestions concise and aligned with these standards.