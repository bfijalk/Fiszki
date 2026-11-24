Feature: Flashcard archiving
  As a logged-in user
  I want to mark a flashcard as learned and archive it
  So that I can see my learning progress and keep my list of flashcards to learn short

  # All scenarios share one database that other scenarios also change, so they create their own
  # flashcards with unique texts and assert on differences from what they remembered before acting,
  # never on absolute numbers. Scenario titles must not contain "empty", "admin" or "demo" because
  # those words select a different seeded test user.

  Background:
    Given the application is running

  Scenario: Every active flashcard has an I learned it button in both views
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving button question" with answer "Archiving button answer"
    Then the flashcard "Archiving button question" should have the I learned it button
    When I toggle the view mode
    Then I should be in "list" view
    And the flashcard "Archiving button question" should have the I learned it button

  Scenario: Cancelling the confirmation keeps the flashcard active
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving cancel question" with answer "Archiving cancel answer"
    And I remember the flashcard counters and the learning progress
    And I click I learned it on the flashcard "Archiving cancel question"
    Then I should see the archive confirmation modal showing "Archiving cancel question"
    When I cancel the archiving
    Then the archive confirmation modal should be closed
    And I should still see the flashcard "Archiving cancel question"
    And the learning progress should be the same as before
    And the archived count should be the same as before

  Scenario: Confirming moves the flashcard to the archived view
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving confirm question" with answer "Archiving confirm answer"
    And I remember the flashcard counters and the learning progress
    And I click I learned it on the flashcard "Archiving confirm question"
    And I confirm the archiving
    Then I should not see the flashcard "Archiving confirm question"
    When I switch to the archived view
    Then I should see the flashcard "Archiving confirm question"
    And the flashcard "Archiving confirm question" should offer Restore and Delete
    And the flashcard "Archiving confirm question" should not have the I learned it button

  Scenario: Archiving raises the learning progress and keeps the counters
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving progress question" with answer "Archiving progress answer"
    And I remember the flashcard counters and the learning progress
    And I click I learned it on the flashcard "Archiving progress question"
    And I confirm the archiving
    Then the learning progress should be higher than before
    And the archived count should be 1 higher than before
    And the total flashcards counter should be the same as before
    And the learning progress should match the archived and total flashcards
    And I should see the flashcard statistics

  Scenario: Restoring brings the flashcard back and lowers the progress
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving restore question" with answer "Archiving restore answer"
    And I click I learned it on the flashcard "Archiving restore question"
    And I confirm the archiving
    And I remember the flashcard counters and the learning progress
    And I switch to the archived view
    And I restore the flashcard "Archiving restore question"
    Then I should not see the flashcard "Archiving restore question"
    And the archived count should be 1 lower than before
    And the learning progress should be lower than before
    And the total flashcards counter should be the same as before
    When I switch to the active view
    Then I should see the flashcard "Archiving restore question"
    And the flashcard "Archiving restore question" should have the I learned it button

  Scenario: An archived flashcard can be deleted
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving delete question" with answer "Archiving delete answer"
    And I click I learned it on the flashcard "Archiving delete question"
    And I confirm the archiving
    And I remember the flashcard counters and the learning progress
    And I switch to the archived view
    And I click delete on card "Archiving delete question"
    Then I should see the delete confirmation modal
    When I confirm the deletion
    Then I should not see the flashcard "Archiving delete question"
    And the total flashcards counter should be 1 lower than before
    And the archived count should be 1 lower than before
    And the learning progress should match the archived and total flashcards

  @demo
  Scenario: Archived view of a user who archived nothing
    When I login with my test user
    And I navigate to the Flashcards page
    And I switch to the archived view
    Then the archived view should say that no flashcards are archived
    And the learning progress should be shown
    And the learning progress should match the archived and total flashcards

  @empty2
  Scenario: Archiving the only flashcard keeps the page usable
    When I login with my test user
    And I navigate to the Flashcards page
    And I create the manual flashcard "Archiving only question" with answer "Archiving only answer"
    And I click I learned it on the flashcard "Archiving only question"
    And I confirm the archiving
    Then the learning progress should be 100 percent
    And the total flashcards counter should be 1
    And the list should say that there are no flashcards to show
    When I switch to the archived view
    Then I should see the flashcard "Archiving only question"
