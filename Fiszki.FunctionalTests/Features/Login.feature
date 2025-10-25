Feature: Login
  As a visitor
  I want to see clear login form behavior
  So that I understand when I can submit

  Background:
    Given the application is running

  Scenario: Login button disabled when fields empty
    Given I am on the Login page
    Then the Login button should be disabled

  Scenario: Login button enabled when both fields filled
    Given I am on the Login page
    When I enter email "user@example.com"
    And I enter password "SomePassword123!"
    Then the Login button should be enabled
