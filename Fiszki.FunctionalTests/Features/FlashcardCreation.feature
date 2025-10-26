Feature: Flashcard Creation and Verification
  As a logged-in user
  I want to create flashcards from source text
  So that I can study the generated content

  Background:
    Given the application is running

  Scenario: Login and create flashcards from source text
    Given I am on the Login page
    When I enter email "test@test.pl"
    And I enter password "test2"
    And I click Login
    Then I should be redirected to the Flashcard Generation page
    When I enter the sample source text
    And I set maximum cards to 5
    And I click Generate Flashcards
    And I click Accept All
    And I click Save Selected
    And I navigate to the Flashcards page
    Then I should see all expected flashcards
    When I click all expected flashcards
    Then all flashcard interactions should be successful
