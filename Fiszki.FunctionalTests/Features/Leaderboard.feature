Feature: Leaderboard
  As a logged-in user
  I want to see how I rank against other users by the flashcards I own and the ones I have learned
  So that I am motivated to create and learn more flashcards and overtake the users above me

  # All scenarios share one database that other scenarios also change, so they assert on the
  # logged-in user's own row, points and banner and on structural properties of the ranking,
  # never on the absolute position of other users.
  #
  # Points are 1 per flashcard plus 3 per archived (learned) flashcard, so an archived flashcard is worth 4.
  # The learning progress is shown next to the points but never decides the order.
  #
  # Not covered here, because they need controlled data that the UI cannot set up, and are covered
  # by unit tests (Fiszki.Tests/Services/LeaderboardServiceTests.cs): the tie-break by who reached
  # the score first (including archiving as a score event), AI and manual flashcards counting equally,
  # the learning progress never changing the order, inactive users being left out, and the banner for
  # the leader.

  Background:
    Given the application is running

  Scenario: Leaderboard menu item is visible after login and opens the leaderboard
    When I login with my test user
    Then I should see the Leaderboard menu item
    When I open the leaderboard
    Then I should be on the leaderboard page
    And the banner should show my position and points

  Scenario: Leaderboard menu item is hidden for visitors
    Given I am on the Home page
    Then I should not see the Leaderboard menu item

  Scenario: Leaderboard page requires login
    When I open the leaderboard address without logging in
    Then I should be asked to log in

  Scenario: My points follow my flashcards
    When I login with my test user
    And I open the leaderboard
    And I remember my leaderboard points
    And I navigate to the Flashcards page
    And I click Add Manual Card
    And I enter question "Leaderboard points question"
    And I enter answer "Leaderboard points answer"
    And I click Create Card
    Then I should see the flashcard "Leaderboard points question"
    When I open the leaderboard
    Then my leaderboard points should be 1 higher than before
    When I navigate to the Flashcards page
    And I click delete on card "Leaderboard points question"
    And I confirm the deletion
    And I open the leaderboard
    Then my leaderboard points should be the same as before

  Scenario: Archiving, restoring and deleting an archived flashcard move my points
    When I login with my test user
    And I open the leaderboard
    And I remember my leaderboard points
    And I navigate to the Flashcards page
    And I create the manual flashcard "Leaderboard archive question" with answer "Leaderboard archive answer"
    And I open the leaderboard
    Then my leaderboard points should be 1 higher than before
    When I navigate to the Flashcards page
    And I click I learned it on the flashcard "Leaderboard archive question"
    And I confirm the archiving
    And I open the leaderboard
    Then my leaderboard points should be 4 higher than before
    And my leaderboard flashcards should be 1 higher than before
    And my leaderboard archived flashcards should be 1 higher than before
    When I navigate to the Flashcards page
    And I switch to the archived view
    And I restore the flashcard "Leaderboard archive question"
    And I open the leaderboard
    Then my leaderboard points should be 1 higher than before
    And my leaderboard flashcards should be 1 higher than before
    And my leaderboard archived flashcards should be 0 higher than before
    When I navigate to the Flashcards page
    And I click I learned it on the flashcard "Leaderboard archive question"
    And I confirm the archiving
    And I switch to the archived view
    And I click delete on card "Leaderboard archive question"
    And I confirm the deletion
    And I open the leaderboard
    Then my leaderboard points should be the same as before
    And my leaderboard flashcards and archived flashcards should be the same as before

  Scenario: Rows show flashcards, archived flashcards, learning progress and points
    When I login with my test user
    And I open the leaderboard
    Then every user on the page should have points equal to flashcards plus 3 per archived flashcard
    And every user on the page should show a learning progress matching their archived and total flashcards
    And the banner should show my own flashcards, archived flashcards and learning progress from my row

  Scenario: Ranking is sorted with unique consecutive positions
    When I login with my test user
    And I open the leaderboard
    Then the points on the page should be sorted from highest to lowest
    And the positions on the page should be unique and consecutive starting from 1
    And users without points should be listed after users with points

  Scenario: Users are shown with masked emails
    When I login with my test user
    And I open the leaderboard
    Then every user on the page should be shown with a masked email
    And the page should not show my full email
    And my own row should be highlighted

  Scenario: Leaderboard is paginated by 30 users
    When I login with my test user
    And I open the leaderboard
    Then I should see 30 users on the page
    And the first position on the page should be 1
    And the previous page button should be disabled
    And the leaderboard should have more than one page
    When I go to the next page
    Then the first position on the page should be 31
    And the previous page button should be enabled
    And the banner should still be shown
    When I go to the last page
    Then the next page button should be disabled
    And the page should list between 1 and 30 users
    When I go to the previous page
    Then the next page button should be enabled

  Scenario: Banner matches the ranking table
    When I login with my test user
    And I open the leaderboard
    Then the banner should match the ranking table

  @empty1
  Scenario: Banner for a user without flashcards
    When I login with my test user
    And I open the leaderboard
    Then my leaderboard points should be 0
    And the banner should match the ranking table
    And the banner should name a rival above me with the same points who reached them first
    And the banner should say I have at least 30 rivals with the same points
