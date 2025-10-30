Feature: Flashcard Deletion
  As a logged-in user
  I want to delete flashcards I no longer need
  So that I can manage my flashcard collection effectively

  Background:
    Given the application is running

  @deletion
  Scenario: Delete a flashcard with confirmation
    When I login with my test user
    Then I should be redirected to the Flashcard Generation page
    When I have dummy flashcards in my account
    And I navigate to the Flashcards page
    Then I should see existing flashcards
    When I click the delete button for the first flashcard
    Then I should see the delete confirmation modal
    When I click Confirm Delete
    Then the flashcard should be deleted
