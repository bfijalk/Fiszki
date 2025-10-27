Feature: Flashcards UI Interactions
  As a logged-in user
  I want to interact with the flashcards UI
  So that I can manage and review my flashcard collection effectively

  Background:
    Given the application is running
    And I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page

  Scenario: View flashcard statistics and filters
    Given I have some existing flashcards
    When I navigate to the Flashcards page
    Then I should see the flashcard statistics
    When I click the "Ai" filter
    Then I should see only AI generated flashcards
    When I click the "Manual" filter
    Then I should see only manual flashcards
    When I clear the filter
    Then I should see all flashcards

  Scenario: Toggle between card and list view
    Given I have some existing flashcards
    When I navigate to the Flashcards page
    Then I should be in "card" view
    When I toggle the view mode
    Then I should be in "list" view
    When I toggle the view mode
    Then I should be in "card" view

  Scenario: Create a manual flashcard
    When I navigate to the Flashcards page
    And I click Add Manual Card
    Then I should see the create card modal
    When I enter question "What is the capital of Poland?"
    And I enter answer "Warsaw"
    And I enter tags "geography, poland, capitals"
    And I click Create Card
    Then I should see the flashcard "What is the capital of Poland?"
    And I should see the flashcard statistics

  Scenario: Create manual flashcard with validation error
    When I navigate to the Flashcards page
    And I click Add Manual Card
    Then I should see the create card modal
    When I click Create Card
    Then I should see a validation error
    When I cancel card creation
    Then the create card modal should be closed

  Scenario: Flip flashcards in card view
    Given I have some existing flashcards
    When I navigate to the Flashcards page
    And I am in card view
    When I flip the card "Heliora"
    Then the card "Heliora" should be flipped
    When I flip the card "Heliora"
    Then the card "Heliora" should show the question

  Scenario: Delete a flashcard with confirmation
    Given I have some existing flashcards
    When I navigate to the Flashcards page
    When I click delete on card "Heliora"
    Then I should see the delete confirmation modal
    When I cancel the deletion
    Then I should still see the flashcard "Heliora"
    When I click delete on card "Heliora"
    And I confirm the deletion
    Then I should not see the flashcard "Heliora"

  Scenario: Empty flashcards state
    Given I have no flashcards
    When I navigate to the Flashcards page
    Then I should see the empty state message
    And I should see the "Generate with AI" button
    And I should see the "Create Manually" button
