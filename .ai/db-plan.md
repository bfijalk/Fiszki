# PostgreSQL Database Schema for 10x-cards MVP

## 1. Tables, Columns, Data Types, and Constraints

### auth.users
This is the built-in Supabase authentication table that we'll leverage (managed by Supabase; not recreated here).

### users (application-level users / authorization anchor)
```sql
-- Roles enumeration (extendable later)
CREATE TYPE user_role AS ENUM ('basic', 'admin');

CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    auth_user_id UUID NOT NULL UNIQUE REFERENCES auth.users(id) ON DELETE CASCADE,
    role user_role NOT NULL DEFAULT 'basic',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);
```

### profiles (optional extension of users for domain stats)
```sql
-- If profiles already existed in a previous migration, adjust instead of re-creating.
-- Updated to reference users(id) instead of auth.users(id) directly.
CREATE TABLE profiles (
    id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    total_cards_generated INTEGER DEFAULT 0 NOT NULL,
    total_cards_accepted INTEGER DEFAULT 0 NOT NULL
);
```

### flashcards
```sql
CREATE TYPE creation_source AS ENUM ('ai', 'manual');

CREATE TABLE flashcards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    front_content TEXT NOT NULL,
    back_content TEXT NOT NULL,
    creation_source creation_source NOT NULL,
    ai_model TEXT,
    original_text_hash TEXT,
    tags TEXT[] DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL
);
```

### learning_progress
```sql
CREATE TABLE learning_progress (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    flashcard_id UUID NOT NULL REFERENCES flashcards(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ease_factor FLOAT NOT NULL DEFAULT 2.5,
    interval INTEGER NOT NULL DEFAULT 0,
    repetitions INTEGER NOT NULL DEFAULT 0,
    next_review_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
    last_review_date TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT now() NOT NULL,
    UNIQUE(flashcard_id)
);
```

## 2. Relationships

- auth.users → users: One-to-one (users.auth_user_id references auth.users.id)
- users → profiles: One-to-one (profiles.id = users.id) (profiles optional)
- users → flashcards: One-to-many (one user owns many flashcards)
- flashcards → learning_progress: One-to-one (each flashcard has one learning progress row)

## 3. Indexes
```sql
-- Index for filtering flashcards by user
CREATE INDEX idx_flashcards_user_id ON flashcards(user_id);

-- Index for joining learning_progress with flashcards
CREATE INDEX idx_learning_progress_flashcard_id ON learning_progress(flashcard_id);

-- Compound index for efficient retrieval during study sessions
CREATE INDEX idx_learning_progress_study_queue ON learning_progress(user_id, next_review_date);

-- Optional: index for role-based queries / admin dashboards
CREATE INDEX idx_users_role ON users(role);
```

## 4. PostgreSQL Row Level Security (RLS) Policies
```sql
-- Enable RLS
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE flashcards ENABLE ROW LEVEL SECURITY;
ALTER TABLE learning_progress ENABLE ROW LEVEL SECURITY;

-- Users: an authenticated user can only see/modify their own users row via auth mapping
CREATE POLICY "User can access own users row"
ON users FOR SELECT USING (auth.uid() = auth_user_id);
CREATE POLICY "User can update own users row"
ON users FOR UPDATE USING (auth.uid() = auth_user_id);
-- (Insert/Delete typically managed by backend service / edge function; adjust as needed)

-- Profiles: user can only access own profile (via users join)
CREATE POLICY "Users can only access their own profile"
ON profiles FOR ALL
USING (EXISTS (SELECT 1 FROM users u WHERE u.id = profiles.id AND u.auth_user_id = auth.uid()));

-- Flashcards: user can only access own flashcards
CREATE POLICY "Users can only access their own flashcards"
ON flashcards FOR ALL
USING (EXISTS (SELECT 1 FROM users u WHERE u.id = flashcards.user_id AND u.auth_user_id = auth.uid()));

-- Learning progress: user can only access own progress rows
CREATE POLICY "Users can only access their own learning progress"
ON learning_progress FOR ALL
USING (EXISTS (SELECT 1 FROM users u WHERE u.id = learning_progress.user_id AND u.auth_user_id = auth.uid()));
```

## 5. Additional Notes & Optional Helpers

### Automatic updated_at maintenance (optional)
```sql
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER set_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER set_profiles_updated_at
BEFORE UPDATE ON profiles
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER set_flashcards_updated_at
BEFORE UPDATE ON flashcards
FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE TRIGGER set_learning_progress_updated_at
BEFORE UPDATE ON learning_progress
FOR EACH ROW EXECUTE FUNCTION set_updated_at();
```

### Auto-create learning_progress row after flashcard insertion (optional)
```sql
CREATE OR REPLACE FUNCTION create_learning_progress()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO learning_progress (flashcard_id, user_id) VALUES (NEW.id, NEW.user_id);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER after_flashcard_insert_create_progress
AFTER INSERT ON flashcards
FOR EACH ROW EXECUTE FUNCTION create_learning_progress();
```

### Simple counters increment example (optional)
(Adjust logic as business rules clarify what counts as "generated" vs "accepted"). If counters remain in profiles use this; alternatively move counters into users to simplify.
```sql
CREATE OR REPLACE FUNCTION increment_profile_counters()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE profiles
    SET total_cards_generated = total_cards_generated + 1
    WHERE id = NEW.user_id; -- only if a profile row exists
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER after_flashcard_insert_increment
AFTER INSERT ON flashcards
FOR EACH ROW EXECUTE FUNCTION increment_profile_counters();
```

### Extension Requirements
Ensure the `pgcrypto` (for gen_random_uuid) extension is enabled:
```sql
CREATE EXTENSION IF NOT EXISTS pgcrypto;
```

## 6. Design Rationale (MVP Focus + Future Authorization)
- Introduced dedicated `users` table to decouple application authorization (roles, status) from Supabase `auth.users` while still linking via `auth_user_id`.
- `profiles` remains an optional extension table; can be merged into `users` later to reduce joins if desired.
- Flashcards & learning_progress now reference `users` directly enabling simpler role-based expansion (e.g., future team sharing, admin moderation).
- RLS updated to pivot on `users.auth_user_id` ensuring consistent ownership checks.
- Roles kept minimal (basic/admin) for MVP; easily extensible to include e.g. `moderator`, `premium`.
- All other MVP simplifications preserved (TEXT content, minimal SM-2 fields, lightweight counters).

---
This updated schema adds future-proof authorization capabilities with minimal additional complexity.
