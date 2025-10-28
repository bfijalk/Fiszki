#!/bin/bash

# Script to test the application in test mode locally
echo "Starting Fiszki application in test mode..."

# Check if OpenRouter API key is provided via environment variable
if [ -n "$OPENROUTER_API_KEY" ]; then
    echo "Using OpenRouter API key from environment variable"
    export OpenRouter__ApiKey="$OPENROUTER_API_KEY"
fi

# Start the application in test mode
dotnet run --project Fiszki.csproj --environment Test --urls "http://localhost:5000" --test-mode &
APP_PID=$!

echo "Application started with PID: $APP_PID"
echo "Waiting for application to start..."

# Wait for the application to be ready
for i in {1..30}; do
    if curl -s http://localhost:5000 > /dev/null 2>&1; then
        echo "Application is ready at http://localhost:5000"
        echo "Test mode is active with in-memory database"
        if [ -n "$OpenRouter__ApiKey" ]; then
            echo "OpenRouter API key configured via environment variable"
        fi
        echo ""
        echo "=== Test Users Available ==="
        echo "1. test@example.com (password: password123) - Basic User (has 3 flashcards)"
        echo "2. admin@example.com (password: admin123) - Admin User (has 1 flashcard)"
        echo "3. demo@example.com (password: demo123) - Demo User (has 2 flashcards)"
        echo "4. empty@example.com (password: empty123) - Basic User (NO flashcards)"
        echo "5. empty2@example.com (password: empty2123) - Basic User (NO flashcards)"
        echo "6. empty3@example.com (password: empty3123) - Basic User (NO flashcards - dedicated for main tests)"
        echo ""
        echo "The database is seeded with:"
        echo "- 6 test users with different roles and flashcard counts"
        echo "- 6 sample flashcards with Polish translations"
        echo "- 5 learning progress entries with various review states"
        echo "- 3 users without flashcards for testing empty page scenarios"
        echo ""
        echo "To stop the application, run: kill $APP_PID"
        echo "Or press Ctrl+C to stop this script and kill the application"
        break
    fi
    echo "Attempt $i: Waiting for application..."
    sleep 2
done

# Check if application started successfully
if ! curl -s http://localhost:5000 > /dev/null 2>&1; then
    echo "Failed to start application"
    kill $APP_PID 2>/dev/null
    exit 1
fi

# Keep the script running and handle Ctrl+C
trap 'echo "Stopping application..."; kill $APP_PID 2>/dev/null; exit 0' INT

echo "Application is running. Press Ctrl+C to stop."
wait $APP_PID
