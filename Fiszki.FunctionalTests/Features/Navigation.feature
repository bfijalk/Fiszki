Feature: Navigation
  As a visitor
  I want to see appropriate navigation links
  So that I know how to access authentication pages

  Background:
    Given the application is running

  Scenario: Nav shows Login link when not authenticated
    Given I am on the Home page
    Then I should see a nav link "Login"


  Scenario: Menu order for a visitor
    Given I am on the Home page
    Then I should see a nav link "Login"
    And the menu should list, in order, "Home | How to Start | Examples | About Technicalities | About Me | Login"

  Scenario: Menu order for a logged-in user
    When I login with my test user
    Then I should see a nav link "Logout"
    And the menu should list, in order, "Home | Flashcards | Generate | Leaderboard | --- | How to Start | Examples | About Technicalities | About Me | --- | Logout"
