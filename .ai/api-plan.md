# REST API Plan

Version: 1.0 (MVP)
Date: 2025-10-14
Scope: Backend REST layer supporting Blazor Server UI for 10x-cards MVP.

---
## 1. Resources
| Resource | DB Table | Description | Notes |
|----------|----------|-------------|-------|
| User | users | Application account + cumulative generation counters | Soft activity flag `is_active`; GDPR delete = cascade to flashcards & progress |
| Flashcard | flashcards | Q/A learning unit (front/back) | Auto learning_progress row (trigger) |
| LearningProgress | learning_progress | SM-2 spaced repetition state per flashcard | Unique on flashcard_id |
| GenerationSession (virtual) | (transient) | Ephemeral AI proposal set before acceptance | Not persisted (proposals only in memory / cache) |
| Stats (virtual) | (aggregations) | Usage & acceptance ratios | Derived from users counters + flashcards |

---
## 2. Endpoints
All endpoints (unless explicitly marked Public) require Authorization: Bearer <JWT>.
Base path: (root) e.g. https://api.example.com
Versioning: Path-based planned (e.g. /v1) – deferred until post-MVP (assumption).

### 2.1 Authentication & User Account

#### POST /users/register (Public)
Create new user and return auth token.
Request:
```json
{
  "email": "user@example.com",
  "password": "PlaintextUserPassword123!"
}
```
Response 201:
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "role": "basic",
  "created_at": "2025-10-14T08:55:00Z",
  "token": {
    "access_token": "jwt-string",
    "expires_in": 3600
  }
}
```
Errors: 400 (validation), 409 (email exists), 500.
Validation: email format RFC 5322; password min 8 chars incl upper/lower/digit.
Side-effects: counters initialized to 0.

#### POST /users/login (Public)
Authenticate user.
Request:
```json
{ "email": "user@example.com", "password": "Password" }
```
Response 200:
```json
{ "access_token": "jwt", "expires_in": 3600 }
```
Errors: 400, 401 (invalid), 423 (if is_active=false), 500.

#### GET /users/me
Return current profile & counters.
Response 200:
```json
{
  "id": "uuid",
  "email": "user@example.com",
  "role": "basic",
  "is_active": true,
  "total_cards_generated": 120,
  "total_cards_accepted": 90,
  "created_at": "2025-09-01T10:00:00Z",
  "updated_at": "2025-10-14T08:55:00Z"
}
```
Errors: 401.

#### DELETE /users/me
GDPR account deletion; cascades to flashcards & learning_progress (ON DELETE CASCADE).
Response 202 (async) or 200 (immediate) – MVP: 200.
```json { "message": "Account deleted" }```
Errors: 401, 409 (pending operations – rare), 500.

#### GET /users/me/export (Optional – stretch)
Return complete user dataset for GDPR portability.
Response 200 (application/json) includes arrays of flashcards & progress.
Errors: 401; 503 (if large export queued) – optional.

### 2.2 Flashcards

#### POST /flashcards
Create manual flashcard.
Request:
```json
{
  "front_content": "What is the capital of France?",
  "back_content": "Paris",
  "tags": ["geography", "europe"]
}
```
Response 201:
```json
{
  "id": "uuid",
  "front_content": "What is the capital of France?",
  "back_content": "Paris",
  "tags": ["geography", "europe"],
  "creation_source": "manual",
  "created_at": "2025-10-14T08:56:00Z",
  "updated_at": "2025-10-14T08:56:00Z"
}
```
Errors: 400 (missing front/back or > max length), 401.
Validation: front/back non-empty trimmed; length <= 2000 chars (assumption to prevent abuse); tags <= 10 each <= 40 chars.
Side: Trigger creates learning_progress row.

#### POST /flashcards/ai/generate
Generate proposals (not persisted) from input text.
Request:
```json
{
  "source_text": "<1000-10000 chars>",
  "max_cards": 25,
  "model": "openrouter:model-id"
}
```
Response 200:
```json
{
  "session_id": "uuid",
  "proposals": [
    { "temp_id": "p1", "front_content": "Q1?", "back_content": "Answer 1", "suggested_tags": ["tag1"] }
  ],
  "model": "openrouter:model-id",
  "generated_count": 12
}
```
Errors: 400 (length <1000 or >10000), 429 (rate limit / AI quota), 502 (AI upstream), 504 (timeout), 401.
Notes: session cached (e.g. IMemoryCache / Redis) 15 min.
Security: sanitize / strip PII before sending to model (future improvement).
Counters: increment `total_cards_generated` by proposals count (or on accept – choose on accept for accuracy) – chosen: on accept.

#### POST /flashcards/ai/accept
Persist selected AI proposals.
Request:
```json
{
  "session_id": "uuid",
  "accepted": [
    { "temp_id": "p1", "front_content": "Q1?", "back_content": "Answer 1", "tags": ["tag1"], "ai_model": "openrouter:model-id" }
  ]
}
```
Response 201:
```json
{
  "persisted": [
    { "id": "uuid", "front_content": "Q1?", "back_content": "Answer 1", "tags": ["tag1"], "creation_source": "ai" }
  ],
  "accepted_count": 10
}
```
Side-effects: increment user counters: total_cards_generated += proposals_in_session; total_cards_accepted += accepted_count (or alternative: generated only accepted – we maintain both metrics so need both numbers: store proposals count earlier in session metadata). Each persisted flashcard gets `ai_model` & `original_text_hash` (hash of normalized source_text SHA256). Learning progress rows auto-created.
Errors: 400 (invalid session / duplicate accept), 401, 404 (session expired), 413 (too many accepted > session proposals), 409 (already accepted), 500.
Idempotency: Accept endpoint should be idempotent using session state flag; repeated call returns same persisted list.

#### GET /flashcards
List user flashcards with filtering.
Query Params:
- page (int>=1 default 1)
- pageSize (int 1-100 default 20)
- sort (front_content|created_at|updated_at) default created_at
- dir (asc|desc) default desc
- q (substring search front/back – simple ILIKE)
- tag (repeatable) e.g. &tag=geography&tag=europe
- source (ai|manual)
Response 200:
```json
{
  "data": [
    {
      "id": "uuid",
      "front_content": "What is the capital of France?",
      "back_content": "Paris",
      "tags": ["geography"],
      "creation_source": "manual",
      "ai_model": null,
      "created_at": "2025-10-14T08:56:00Z",
      "updated_at": "2025-10-14T08:56:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 134,
  "totalPages": 7
}
```
Errors: 400 (bad params), 401.
Performance: Index on (user_id) plus optional trigram/GIN for future full-text (deferred).

#### GET /flashcards/{id}
Response 200 as single resource (+ maybe learning progress snapshot if ?include=progress).
Errors: 404, 401.

#### PUT /flashcards/{id}
Full update (front/back/tags). Only content & tags editable.
Request:
```json
{ "front_content": "Updated Q?", "back_content": "Updated A", "tags": ["revised"] }
```
Response 200 with updated object.
Headers: If-Match: <etag> (optional concurrency). Respond 412 if mismatch.
Errors: 400, 404, 409 (etag), 401.

#### PATCH /flashcards/{id}
Partial update (e.g. only tags). JSON Merge Patch semantics.
Request example:
```json
{ "tags": ["history"] }
```
Response 200.

#### DELETE /flashcards/{id}
Confirmation handled client-side.
Response 200 `{ "message": "Flashcard deleted" }`.
Errors: 404, 401.
Cascade removes learning_progress.

#### POST /flashcards/batch/delete (Optional convenience)
Request:
```json
{ "ids": ["uuid1", "uuid2"] }
```
Response 200 with counts.
Errors: 400, 401.

### 2.3 Learning / Spaced Repetition

#### GET /learning/schedule
Return due flashcards ordered by next_review_date and priority.
Query:
- limit (default 30, max 100)
- after (ISO timestamp optional – for pagination cursor)
Response 200:
```json
{
  "due_count": 42,
  "items": [
    {
      "flashcard_id": "uuid",
      "front_content": "Q?",
      "back_preview": null,
      "ease_factor": 2.5,
      "interval": 0,
      "repetitions": 0,
      "next_review_date": "2025-10-14T09:00:00Z"
    }
  ],
  "cursor": "2025-10-14T09:05:00Z"
}
```
Errors: 401.

#### GET /learning/flashcards/{flashcard_id}
Return full card with current progress (used inside session after reveal).
Response 200:
```json
{
  "flashcard_id": "uuid",
  "front_content": "Q?",
  "back_content": "A",
  "progress": {
    "ease_factor": 2.5,
    "interval": 0,
    "repetitions": 0,
    "next_review_date": "2025-10-14T09:00:00Z"
  }
}
```
Errors: 404, 401.

#### POST /learning/review
Submit a review result (SM-2 rating 0-5).
Request:
```json
{
  "flashcard_id": "uuid",
  "rating": 4,
  "reviewed_at": "2025-10-14T09:10:00Z"
}
```
Response 200:
```json
{
  "flashcard_id": "uuid",
  "new_interval": 6,
  "new_ease_factor": 2.6,
  "new_repetitions": 2,
  "next_review_date": "2025-10-20T09:10:00Z"
}
```
Errors: 400 (invalid rating), 404, 409 (already reviewed too soon), 401.
Algorithm: Standard SM-2 (ease factor min 1.3). Update last_review_date, next_review_date, interval, repetitions, ease_factor in transaction.

### 2.4 Stats & Monitoring

#### GET /stats/flashcards
Aggregated per-user stats.
Response 200:
```json
{
  "total_manual": 40,
  "total_ai": 120,
  "generated_proposals_total": 200,
  "accepted_ai": 120,
  "acceptance_rate": 0.6,
  "ai_usage_ratio": 0.75
}
```
Errors: 401.

#### GET /stats/learning
Session readiness summary.
Response 200:
```json
{
  "due_now": 42,
  "due_today": 60,
  "scheduled_future": 300,
  "last_reviewed_at": "2025-10-14T07:00:00Z"
}
```
Errors: 401.

### 2.5 System

#### GET /health (Public minimal)
Liveness (no auth) returns build & db connectivity flag.
Response 200: `{ "status": "ok", "db": "up", "time": "2025-10-14T09:00:00Z" }`

---
## 3. Request & Response Conventions
- Content-Type: application/json; charset=utf-8
- Error format:
```json
{ "error": { "code": "RESOURCE_NOT_FOUND", "message": "Flashcard not found", "details": null, "trace_id": "uuid" } }
```
- Pagination (page/pageSize) OR cursor (schedule). Provide both `totalItems` and `totalPages` for page-based endpoints.
- Timestamps: ISO 8601 UTC (Z).
- IDs: UUID v4.
- ETag: hash of serialized flashcard fields (PUT concurrency) – optional header support.

---
## 4. Validation Rules (API Layer)
| Entity | Field | Rules |
|--------|-------|-------|
| User | email | required, valid format, unique (case-insensitive) |
| User | password | required (register), min 8, at least 1 upper, 1 lower, 1 digit (policy assumption) |
| Flashcard | front_content/back_content | required, trimmed length 1..2000 (assumption) |
| Flashcard | tags | optional array size ≤10, each 1..40, distinct (case-insensitive) |
| Flashcard | creation_source | enum {ai, manual} (db enforced) |
| LearningProgress | rating (review) | int 0..5 |
| LearningProgress | flashcard_id | must belong to current user |
| AI Generation | source_text | length 1000..10000 chars (PRD), UTF-8 |
| Batch Delete | ids | 1..100 items |

Database constraints supplement (UNIQUE email, FK cascades, UNIQUE(flashcard_id)).

---
## 5. Business Logic Mapping
| Feature (PRD) | Endpoint(s) | Logic Highlights |
|---------------|-------------|------------------|
| US-001 Register | POST /users/register | Hash password (Argon2id), issue JWT, initialize counters |
| US-002 Login | POST /users/login | Verify hash, issue JWT |
| US-003 Generate AI | POST /flashcards/ai/generate | Validate text length, call OpenRouter, produce proposals, cache session |
| US-004 Approve / Edit Proposals | POST /flashcards/ai/accept, PUT/PATCH /flashcards/{id} | Persist accepted subset; counters updated; editing allowed afterward |
| US-005 Edit Flashcards | PUT/PATCH /flashcards/{id} | Concurrency with ETag; validation |
| US-006 Delete Flashcards | DELETE /flashcards/{id}, POST /flashcards/batch/delete | Cascade removes learning progress |
| US-007 Manual Create | POST /flashcards | Set creation_source=manual, auto progress row |
| US-008 Learning Session | GET /learning/schedule, GET /learning/flashcards/{id}, POST /learning/review | Filter due by next_review_date <= now(); apply SM-2 update |
| US-009 Secure Access | (all authenticated endpoints) | Ownership enforced by user_id filter in queries |
| Stats Acceptance | GET /stats/flashcards | Derive acceptance_rate = accepted_ai / generated_proposals_total |

---
## 6. Authentication & Authorization
Mechanism: JWT (HS256 initially; RS256 recommended for multi-service future). Secret in environment variable.
Flow:
1. Register/Login returns access_token (TTL 1h).
2. (Optional future) Refresh tokens (secure http-only cookie) – deferred MVP.
3. Middleware validates token, sets UserId claim.
4. Repositories always filter by userId for flashcards/progress.
Password Hashing: Argon2id (libsodium / Isopoh Argon2). Pepper (optional) + per-user salt.
Account Deletion: Single transaction deleting from users (cascade) + immediate token invalidation (server-side deny-list optional if adopting refresh tokens).

Authorization Rules:
- Users can only access records where record.user_id == current user id.
- Admin role (future) may access aggregated stats across users (not in MVP endpoints yet).

---
## 7. Security Measures
| Concern | Approach |
|---------|----------|
| Transport | Enforce HTTPS (HSTS) |
| Brute force login | Rate limit /users/login (e.g. 5/min/IP + exponential backoff) |
| AI abuse (long text) | Enforce 10k char upper bound, strip HTML, reject binary |
| Injection | Parameterized EF Core queries; no raw SQL without interpolation safeguards |
| Data isolation | user_id scoping everywhere |
| Secrets | Stored in environment / user-secrets (no commit) |
| Rate limiting | Global (e.g. 1000 req / 15m / user) + sensitive endpoints stricter |
| Logging | Exclude passwords / source_text from logs (store hash only) |
| DOS on generation | Queue + timeout (e.g. 30s) + circuit breaker on upstream errors |
| Replay (accept endpoint) | Session idempotency flag |
| Concurrency | ETag on flashcard updates; 412 on mismatch |
| PII GDPR | DELETE /users/me + optional export endpoint |

---
## 8. Performance & Pagination
- List endpoints default pageSize=20; cap 100 to protect memory.
- Index usage: `idx_flashcards_user` ensures fast user scoping; add composite index (user_id, created_at DESC) if query profile indicates.
- Learning schedule uses `idx_learning_progress_queue (user_id, next_review_date)` to fetch due cards efficiently (WHERE user_id=? AND next_review_date <= now()). Limit + ORDER BY next_review_date.
- Caching: AI proposal sessions in memory/cache; optional caching of stats for 30s.
- N+1 avoidance: projection queries (no lazy loading) selecting only required fields.

---
## 9. Error Codes Catalog
| Code | HTTP | Message Example |
|------|------|-----------------|
| VALIDATION_ERROR | 400 | "front_content required" |
| UNAUTHORIZED | 401 | "Missing or invalid token" |
| FORBIDDEN | 403 | (Reserved) |
| NOT_FOUND | 404 | "Flashcard not found" |
| CONFLICT | 409 | "Resource version mismatch" |
| RATE_LIMITED | 429 | "Too many generation requests" |
| UPSTREAM_FAILURE | 502 | "AI provider error" |
| TIMEOUT | 504 | "AI generation timed out" |
| SERVER_ERROR | 500 | "Unexpected error" |

---
## 10. SM-2 Algorithm Implementation (Summary)
Input: rating 0..5
If rating <3: repetitions=0; interval=1; (ease_factor stays or decreases)
Else: repetitions +=1; interval progression: (1,6, previous_interval * EF) with rounding.
Ease factor update: EF' = EF + (0.1 - (5 - rating)*(0.08 + (5 - rating)*0.02)); clamp EF' >=1.3
next_review_date = now + interval days.
All updates in single transaction. Return new values.

---
## 11. Data Models (API Layer DTOs)
UserDto: id, email, role, is_active, total_cards_generated, total_cards_accepted, created_at, updated_at.
FlashcardDto: id, front_content, back_content, tags[], creation_source, ai_model, created_at, updated_at.
LearningProgressDto: flashcard_id, ease_factor, interval, repetitions, next_review_date, last_review_date.
ReviewResultDto: flashcard_id, new_interval, new_ease_factor, new_repetitions, next_review_date.
StatsFlashcardsDto, StatsLearningDto.

---
## 12. Logging & Observability
- Correlation: trace_id UUID per request (returned in error responses).
- Log levels: Info (endpoint start/end), Warning (validation anomalies), Error (exceptions), Debug (disabled production) – exclude PII.
- Metrics (future): count requests per endpoint, latency, AI generation latency, acceptance ratio.

---
## 13. Deployment & Versioning Notes
- Initial release without explicit /v1 path; add version header `X-API-Version: 1.0`.
- Future breaking changes introduce /v2 while retaining /v1 for deprecation window.

---
## 14. Open Questions / Assumptions
| Topic | Assumption | Action |
|-------|------------|--------|
| Refresh tokens | Deferred | Revisit post-MVP |
| Multi-language cards | Not supported MVP | Future field additions |
| Tag normalization | Case-insensitive, stored lowercase | Enforce at API layer |
| Source text storage | Not stored (privacy) only hash | Confirm with legal/privacy |
| AI model tracking | Store model string in flashcards.ai_model | Already in schema |

---
## 15. Implementation Checklist (Internal)
- [ ] DTOs & Validators (FluentValidation)
- [ ] Authentication middleware (JWT)
- [ ] Error handling middleware (uniform error envelope)
- [ ] Flashcard repository (scoped by user)
- [ ] AI service abstraction (OpenRouter client with timeout & retry)
- [ ] Generation session cache (IMemoryCache)
- [ ] SM-2 service
- [ ] Stats service (aggregations)
- [ ] Rate limiting (ASP.NET Core rate limiter)
- [ ] Unit tests: validators, SM-2 calculations, AI service fallback
- [ ] Integration tests: flashcard CRUD, review flow

---
## 16. Sample Error Response
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "source_text length must be between 1000 and 10000",
    "details": { "source_text": "Too short" },
    "trace_id": "c1b4d7e4-9d43-47af-9f6c-a4c6ae0a3d62"
  }
}
```

---
## 17. Summary
Plan covers: full CRUD, AI generation lifecycle, spaced repetition operations, counters for metrics, GDPR deletion, validation, security, and performance considerations aligned with PostgreSQL schema and PRD requirements.

END OF DOCUMENT

