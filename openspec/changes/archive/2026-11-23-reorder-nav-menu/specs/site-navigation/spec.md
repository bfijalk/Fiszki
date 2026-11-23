# Spec Delta

## Purpose

Defines the order and grouping of the side menu so that Home is always first and the items a
logged-in user works with sit above the items every visitor sees.

## ADDED Requirements

### Requirement: Home is the first menu item
The system SHALL show Home as the first item of the side menu for every user, whether or not they
are logged in.

#### Scenario: Visitor
- **WHEN** a visitor who is not logged in views the side menu
- **THEN** Home is the first item

#### Scenario: Logged-in user
- **WHEN** a logged-in user views the side menu
- **THEN** Home is the first item

### Requirement: Logged-in items above public items
The system SHALL show a logged-in user the items available only after login (Flashcards, Generate,
Leaderboard) directly after Home and above the items that every visitor sees (How to Start, Examples,
About Technicalities, About Me), in that order within each group.

#### Scenario: Order for a logged-in user
- **WHEN** a logged-in user views the side menu
- **THEN** the items appear in this order: Home, Flashcards, Generate, Leaderboard, How to Start, Examples, About Technicalities, About Me, Logout

#### Scenario: Every logged-in item is above every public item
- **WHEN** a logged-in user views the side menu
- **THEN** Flashcards, Generate and Leaderboard are each shown above How to Start, Examples, About Technicalities and About Me

### Requirement: Logout is the last menu item
The system SHALL show Logout as the last item of the side menu for a logged-in user, below the
public items.

#### Scenario: Logout last
- **WHEN** a logged-in user views the side menu
- **THEN** Logout is the last item, below About Me

### Requirement: Separators between menu groups
The system SHALL show a separator between the logged-in group and the public group, and a second
separator between the public group and Logout, for a logged-in user. The system SHALL NOT show
separators to a visitor who is not logged in.

#### Scenario: Separators for a logged-in user
- **WHEN** a logged-in user views the side menu
- **THEN** a separator is shown between Leaderboard and How to Start, and another between About Me and Logout

#### Scenario: No separators for a visitor
- **WHEN** a visitor who is not logged in views the side menu
- **THEN** no separator is shown

### Requirement: Visitor menu order is unchanged
The system SHALL show a visitor who is not logged in the items Home, How to Start, Examples, About
Technicalities, About Me and Login, in that order, and SHALL NOT show logged-in items or Logout to
them.

#### Scenario: Order for a visitor
- **WHEN** a visitor who is not logged in views the side menu
- **THEN** the items appear in this order: Home, How to Start, Examples, About Technicalities, About Me, Login

#### Scenario: Menu after logging out
- **WHEN** a logged-in user uses Logout
- **THEN** the menu shows the visitor order and no logged-in items
