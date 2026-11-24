-- Migration 20261124100000_AddFlashcardArchivedAt (change add-flashcard-archiving)
-- Adds the nullable column that marks a flashcard as learned (archived).
-- Existing flashcards get NULL, so they all stay active and nobody's points change.
--
-- Safe to run more than once. Run it BEFORE deploying the new application version:
-- the application does not apply migrations itself.
--
-- Rollback (archived flashcards become active again, leaderboard points drop by 3 per archived card):
--   ALTER TABLE flashcards DROP COLUMN IF EXISTS archived_at;
--   DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261124100000_AddFlashcardArchivedAt';

BEGIN;

ALTER TABLE flashcards
    ADD COLUMN IF NOT EXISTS archived_at timestamp with time zone NULL;

-- Record the migration in the EF Core history table, if the database has one.
DO $$
BEGIN
    IF to_regclass('public."__EFMigrationsHistory"') IS NOT NULL THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        SELECT '20261124100000_AddFlashcardArchivedAt', '10.0.15'
        WHERE NOT EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = '20261124100000_AddFlashcardArchivedAt');
    ELSE
        RAISE NOTICE '__EFMigrationsHistory does not exist: only the column was added.';
    END IF;
END
$$;

COMMIT;

-- Check afterwards: should return one row (data_type = timestamp with time zone, is_nullable = YES).
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'flashcards' AND column_name = 'archived_at';
