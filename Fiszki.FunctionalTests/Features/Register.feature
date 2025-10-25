Feature: Register
  As a prospective user
  I want to be informed about mistakes
  So that I can correct them easily

  Background:
    Given the application is running

  Scenario: Password mismatch shows message
    Given I am on the Register page
    When I enter register email "newuser@example.com"
    And I enter register password "Secret123!"
    And I enter register confirm password "Different123!"
    Then I should see password mismatch message

