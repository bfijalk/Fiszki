Feature: Flashcard Generation Validation
  As a logged-in user
  I want to generate flashcards and see them appear on the generation page
  So that I can verify the AI generation process works correctly

  Background:
    Given the application is running

  Scenario: Generate flashcards and validate they appear on generation page
    When I login with my test user
    When I click "Generate" button on navbar
    When I enter the sample source text
    And I set maximum cards to 5
    And I click Generate Flashcards
    Then I should see the "Generation completed" message
    And I should see generated flashcards on the page
    And I should see the correct flashcard count displayed
    And the generated flashcards should contain expected content
    When I click Accept All
    Then all flashcards should be selected
    And the Save Selected button should be enabled
