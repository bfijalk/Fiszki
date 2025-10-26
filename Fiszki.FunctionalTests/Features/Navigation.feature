Feature: Navigation
  As a visitor
  I want to see appropriate navigation links
  So that I know how to access authentication pages

  Background:
    Given the application is running

  Scenario: Nav shows Login link when not authenticated
    Given I am on the Home page
    Then I should see a nav link "Login"

