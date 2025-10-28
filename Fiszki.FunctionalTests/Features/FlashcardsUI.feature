Feature: Flashcards UI Interactions
  As a logged-in user
  I want to interact with the flashcards UI
  So that I can manage and review my flashcard collection effectively

  Background:
    Given the application is running

  Scenario: Empty flashcards state
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should see the empty state message
    And I should see the "Generate with AI" button
    And I should see the "Create Manually" button

  Scenario: Create a manual flashcard
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
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
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    And I click Add Manual Card
    Then I should see the create card modal
    When I click Create Card
    Then I should see a validation error
    When I cancel card creation
    Then the create card modal should be closed

  Scenario: View flashcard statistics and filters with generated flashcards
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I enter the sample source text
    And I set maximum cards to 3
    And I click Generate Flashcards
    And I click Accept All
    And I click Save Selected
    When I navigate to the Flashcards page
    Then I should see the flashcard statistics
    When I click the "Ai" filter
    Then I should see only AI generated flashcards
    When I click the "Manual" filter
    Then I should see only manual flashcards
    When I click the "Manual" filter
    Then I should see all flashcards

  Scenario: Toggle between card and list view
    Given I am on the Login page
    When I login with my test user
    And I have dummy flashcards in my account
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should be in "card" view
    When I toggle the view mode
    Then I should be in "list" view
    When I toggle the view mode
    Then I should be in "card" view

  Scenario: Flip flashcards in card view
    Given I am on the Login page
    When I login with my test user
    And I have dummy flashcards in my account
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    And I am in card view
    When I flip the card "Heliora"
    Then the card "Heliora" should be flipped
    When I flip the card "Heliora"
    Then the card "Heliora" should show the question
