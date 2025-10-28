Feature: Flashcards UI Interactions
  As a logged-in user
  I want to interact with the flashcards UI
  So that I can manage and review my flashcard collection effectively

  Background:
    Given the application is running

  @empty1
  Scenario: Empty flashcards state
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should see the empty state message
    And I should see the "Generate with AI" button
    And I should see the "Create Manually" button

  @empty3
  Scenario: Create a manual flashcard from empty state
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

  @empty2
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

  Scenario: View existing flashcards and statistics
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should see existing flashcards
    And I should see the flashcard statistics
    And I should see flashcards like "Hello", "Thank you", "Good morning"

  @demo
  Scenario: View flashcard statistics and filters with more flashcards
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should see existing flashcards
    And I should see the flashcard statistics
    And I should see flashcards like "Book", "Water"

  Scenario: Toggle between card and list view
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    Then I should see existing flashcards
    Then I should be in "card" view
    When I toggle the view mode
    Then I should be in "list" view
    When I toggle the view mode
    Then I should be in "card" view

@ignore
  Scenario: Flip flashcards in card view
    Given I am on the Login page
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I navigate to the Flashcards page
    And I am in card view
    When I flip any available card
    Then the card should be flipped
    When I flip the same card again
    Then the card should show the question
