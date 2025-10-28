Feature: Manual Flashcard Creation
  As a logged-in user
  I want to create flashcards manually
  So that I can add custom content to study

  Background:
    Given the application is running

  Scenario: Create a manual flashcard successfully
    Given I am on the Login page
    When I login with my test user
    And I navigate to the Flashcards page
    When I click Add Manual Card
    Then I should see the create card modal
    When I enter question "Dodaj pytanie na przod formatki"
    And I enter answer "Dodaj odpowiedz na koncu formatki"
    And I enter tags "tagi1"
    And I click Create Card
    Then the create card modal should be closed
    And I should see the flashcard "Dodaj pytanie na przod formatki"

  Scenario: Attempt to create flashcard with missing required fields
    Given I am on the Login page
    When I login with my test user
    And I navigate to the Flashcards page
    When I click Add Manual Card
    Then I should see the create card modal
    When I enter question ""
    And I enter answer "Some answer"
    And I click Create Card
    Then I should see a validation error

  Scenario: Cancel manual flashcard creation
    Given I am on the Login page
    When I login with my test user
    And I navigate to the Flashcards page
    When I click Add Manual Card
    Then I should see the create card modal
    When I enter question "Test question"
    And I enter answer "Test answer"
    And I cancel card creation
    Then the create card modal should be closed


