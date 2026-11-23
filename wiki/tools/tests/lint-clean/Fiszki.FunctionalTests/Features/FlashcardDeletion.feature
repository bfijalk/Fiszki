Feature: Flashcard deletion

  Scenario: Delete a flashcard with confirmation
    Given I have a flashcard
    When I delete it and confirm
    Then the flashcard is removed
