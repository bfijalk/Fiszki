# Database Cleanup for Functional Tests

This document explains the database cleanup functionality implemented for the Fiszki functional tests.

## Overview

The database cleanup system ensures that each test scenario starts with a clean state by removing flashcards data for the test user (`test@test.pl`) while preserving the user account itself.

## How It Works

### Cleanup Timing
- **Before each scenario**: Removes any existing flashcards to ensure a clean starting state
- **After successful scenarios**: Cleans up generated flashcards to maintain cleanliness
- **After failed scenarios**: Preserves flashcard data to help with debugging

### What Gets Cleaned
- All flashcards belonging to the test user
- Associated learning progress records
- **User account is preserved** - only flashcard data is removed

## Configuration

The cleanup behavior can be configured via `appsettings.json`:

```json
{
  "DatabaseCleanup": {
    "ConnectionString": "Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres",
    "TestUserEmail": "test@test.pl",
    "EnableCleanup": true,
    "CleanupOnScenarioStart": true,
    "CleanupOnScenarioSuccess": true,
    "PreserveDataOnFailure": true
  }
}
```

You can also override the connection string using the environment variable:
```bash
export FISZKI_DB_CONNECTION_STRING="Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres"
```

## Components

### DatabaseCleanupService
- Handles the actual database operations
- Connects directly to PostgreSQL using Entity Framework Core
- Includes error handling to prevent cleanup failures from breaking tests

### DatabaseCleanupHooks
- SpecFlow hooks that orchestrate when cleanup happens
- Runs before and after scenarios based on configuration
- Provides detailed logging of cleanup operations

## Benefits

1. **Consistent Test State**: Each test starts with the same clean database state
2. **Debugging Friendly**: Failed tests preserve data for investigation
3. **Safe Operations**: Only targets test user data, never production data
4. **Comprehensive**: Removes both flashcards and related learning progress
5. **Configurable**: Behavior can be adjusted without code changes

## Usage

The cleanup happens automatically when running functional tests. No additional setup is required beyond ensuring:

1. The test user (`test@test.pl`) exists in the database
2. The database connection string is correctly configured
3. The application is running on the expected URL

## Logging

The cleanup service provides detailed console logging:
- `[Database Cleanup]` prefix for all cleanup-related messages
- Reports the number of flashcards and learning progress records cleaned
- Warns if the test user is not found
- Reports errors without failing tests
