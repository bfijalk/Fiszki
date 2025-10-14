# API Endpoint Integration Plan: Services Integration for Blazor Server Application

## 1. Endpoint Overview
This plan focuses solely on integrating the service layer, database operations, and Blazor frontend. The business logic is implemented in the `Fiszki.Services` project, persistence is managed by the `Fiszki.Database` project, and the Blazor UI in the `Fiszki` project consumes these services.

## 2. Request Details
- **HTTP Methods & URL Structure**
  - Endpoints for user registration, login, profile management in `/users/*`.
  - Endpoints for flashcard operations in `/flashcards/*` (manual, AI generation and acceptance).
  - Endpoints for learning session and spaced repetition in `/learning/*`.
  - Endpoints for statistics in `/stats/*`.
- **Parameters**
  - Required fields (e.g., email, password, flashcard content).
  - Optional fields such as pagination, query filters, and ETag for concurrency.

## 3. Used Types
- **DTOs:** `UserDto`, `FlashcardDto`, `LearningProgressDto`, `ReviewResultDto`, `StatsFlashcardsDto`, `StatsLearningDto`.
- **Command Models:** `RegisterUserCommand`, `LoginCommand`, `CreateFlashcardCommand`, `GenerateAIFlashcardsCommand`, `AcceptedFlashcardProposal`, `AcceptAIFlashcardsCommand`, `UpdateFlashcardCommand`, `DeleteFlashcardCommand`, `DeleteFlashcardsBatchCommand`, `SubmitReviewCommand`.

## 4. Response Details
- **Success Response Codes:**
  - 200 for GET, PATCH, review submissions and deletions.
  - 201 for POST endpoints that create new resources.
- **Error Handling:**
  - Return 400, 401, 404, 409, 429, 502/504 as applicable.
  - Use a uniform JSON error envelope with a trace\_id.

## 5. Data Flow & Integration
1. **Blazor Frontend (`Fiszki` project):**
   - Sends requests to controllers in `Fiszki.Services`.
   - Receives responses containing DTOs.
2. **Service Layer (`Fiszki.Services` project):**
   - Validates incoming requests using FluentValidation.
   - Processes business logic for user management, flashcard operations, AI generation, and learning sessions.
   - Delegates data persistence and queries to the `Fiszki.Database` project.
3. **Data Layer (`Fiszki.Database` project):**
   - Implements EF Core repositories.
   - Maps to PostgreSQL tables using the provided schema.
   - Enforces database constraints, relationships and triggers.
4. **Integration Points:**
   - Exchange of data via DTOs and command models.
   - Authentication managed by JWT middleware in `Fiszki.Services`.
   - AI generation session caching implemented within `Fiszki.Services` (using IMemoryCache or Redis).
   - Concurrency control via ETag headers on flashcard updates.

## 6. Security Considerations
- Enforce JWT authentication and validate each token in `Fiszki.Services`.
- Ensure user data isolation in all service and repository calls.
- Validate and sanitize user input using FluentValidation.
- Limit access to data by applying user\_id filters in `Fiszki.Database` operations.

## 7. Error Handling
- Centralized error handling in `Fiszki.Services` with uniform error envelopes.
- Log errors with a unique trace\_id while omitting sensitive data.
- Handle ETag-based concurrency conflicts, mapping them to proper HTTP status codes.

## 8. Integration Steps
1. Define the shared DTOs and command models in the service layer.
2. Implement the business logic services in the `Fiszki.Services` project:
   - UserService: Handles registration, login, profile retrieval, and deletion.
   - FlashcardService: Manages creation, updating, deletion, and AI flashcard proposals.
   - AIService: Integrates with the OpenRouter AI Gateway for flashcard generation.
   - LearningService: Implements spaced repetition logic using the SM-2 algorithm.
3. Implement EF Core repositories in the `Fiszki.Database` project according to the provided schema.
4. Wire up JWT-based authentication middleware in `Fiszki.Services` to secure all endpoints.
5. Expose the required controller endpoints in `Fiszki.Services` to allow the Blazor UI to perform CRUD operations and manage learning sessions.
6. Integrate the Blazor frontend in the `Fiszki` project with API client calls to invoke services from `Fiszki.Services`.
7. Ensure proper plumbing between projects so that service calls correctly invoke the underlying database operations.
8. Verify integration points by manually triggering operations from the Blazor UI.

Save this plan as `.ai/view-implementation-plan.md`.

