# PostgreSQL Database Schema for 10x-cards MVP (Standalone EF Core Version)

Authorization and authentication are handled in the .NET backend services; PostgreSQL is accessed directly via EF Core.

## 1. Tables, Columns, Data Types, and Constraints

### Enumerations
```sql
CREATE TYPE user_role AS ENUM ('basic', 'admin');
CREATE TYPE creation_source AS ENUM ('ai', 'manual');
```

### users
Single table for identity + domain counters.
```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email CITEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role user_role NOT NULL DEFAULT 'basic',
    is_active BOOLEAN NOT NULL DEFAULT true,
    total_cards_generated INTEGER NOT NULL DEFAULT 0,
    total_cards_accepted INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
```
(Requires: `CREATE EXTENSION IF NOT EXISTS citext;`)

### flashcards
```sql
CREATE TABLE flashcards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    front_content TEXT NOT NULL,
    back_content TEXT NOT NULL,
    creation_source creation_source NOT NULL,
    ai_model TEXT,
    original_text_hash TEXT,
    tags TEXT[] NOT NULL DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

### learning_progress
```sql
CREATE TABLE learning_progress (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flashcard_id UUID NOT NULL REFERENCES flashcards(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ease_factor DOUBLE PRECISION NOT NULL DEFAULT 2.5,
    interval INTEGER NOT NULL DEFAULT 0,
    repetitions INTEGER NOT NULL DEFAULT 0,
    next_review_date TIMESTAMPTZ NOT NULL DEFAULT now(),
    last_review_date TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (flashcard_id)
);
```

## 2. Relationships
- users → flashcards: 1:N
- users → learning_progress: 1:N
- flashcards → learning_progress: 1:1

## 3. Indexes
```sql
CREATE INDEX idx_flashcards_user ON flashcards(user_id);
CREATE INDEX idx_learning_progress_card ON learning_progress(flashcard_id);
CREATE INDEX idx_learning_progress_queue ON learning_progress(user_id, next_review_date);
CREATE INDEX idx_users_email_ci ON users(email);
```

## 4. Optional Triggers
```sql
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END; $$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_flashcards_updated BEFORE UPDATE ON flashcards FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_learning_progress_updated BEFORE UPDATE ON learning_progress FOR EACH ROW EXECUTE FUNCTION set_updated_at();
```

Automatic progress row creation (optional):
```sql
CREATE OR REPLACE FUNCTION create_learning_progress()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO learning_progress (flashcard_id, user_id) VALUES (NEW.id, NEW.user_id);
  RETURN NEW;
END; $$ LANGUAGE plpgsql;

CREATE TRIGGER trg_flashcard_after_insert
AFTER INSERT ON flashcards FOR EACH ROW EXECUTE FUNCTION create_learning_progress();
```

## 5. Security & Authorization (Application Layer)
- Auth & ownership checks enforced in backend services.
- Strong password hashing (e.g. Argon2id) recommended.
- Optionally enable RLS later if needed.

## 6. Migration Strategy
1. Add entities (users, flashcards, learning_progress)
2. Create initial migration
3. Apply to environments

## 7. Notes
- Enums stored as text for readability.
- `tags` uses Postgres text[] (maps to List<string> in EF Core).
- Add review history table later if analytics required.

## 8. Extensions
```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS citext;
```

---
Ready for implementation via EF Core migrations once approved.
