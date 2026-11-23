# Spec Delta

## Purpose

Motivates users to create more flashcards by ranking active users by the number of flashcards
they own and showing each user how far they are from the user directly above them.

## ADDED Requirements

### Requirement: Leaderboard access and menu entry
The system SHALL show a "Leaderboard" item in the side menu to logged-in users only, and the
`/leaderboard` page SHALL be available only to logged-in users.

#### Scenario: Logged-in user sees the menu item
- **WHEN** a logged-in user views the side menu
- **THEN** a "Leaderboard" item is visible and opens the leaderboard page

#### Scenario: Logged-out visitor
- **WHEN** a visitor who is not logged in views the side menu or opens `/leaderboard`
- **THEN** the menu shows no "Leaderboard" item and the page is not shown, consistent with the other protected pages

### Requirement: Points equal the user's flashcards
The system SHALL give each user one point per flashcard they currently own, counting AI-generated and manually created flashcards equally.

#### Scenario: Both creation sources count
- **GIVEN** a user owns 3 AI-generated and 2 manually created flashcards
- **WHEN** the leaderboard is computed
- **THEN** the user has 5 points

#### Scenario: Deleting a flashcard removes a point
- **GIVEN** a user with 5 points
- **WHEN** the user deletes one flashcard and the leaderboard is shown again
- **THEN** the user has 4 points

### Requirement: Ranking order with unique positions
The system SHALL order users by points from highest to lowest and SHALL assign each user a unique position (1, 2, 3, ...). Users with equal points SHALL be ordered by who reached the score first, that is, the user whose most recent flashcard was created earlier ranks higher.

#### Scenario: Higher points rank higher
- **GIVEN** user A has 10 points and user B has 7 points
- **WHEN** the leaderboard is shown
- **THEN** A is above B

#### Scenario: Tie broken by who reached the score first
- **GIVEN** users A and B both have 7 points and A's most recent flashcard is older than B's
- **WHEN** the leaderboard is shown
- **THEN** A has the better position and the two positions are different

### Requirement: Users with no points
The system SHALL list active users with 0 points after all users with at least one point, and SHALL still give them unique positions.

#### Scenario: Zero-point user listed last
- **GIVEN** a user with 0 flashcards and other users with at least 1 flashcard
- **WHEN** the leaderboard is shown
- **THEN** the zero-point user appears below every user with points

### Requirement: Inactive users are excluded
The system SHALL leave users whose account is inactive out of the leaderboard, both from the list and from position numbering.

#### Scenario: Inactive user not listed
- **GIVEN** an inactive user with 20 points
- **WHEN** the leaderboard is shown
- **THEN** that user is not listed and does not take a position

### Requirement: Masked user identity
The system SHALL identify users in the leaderboard only by a masked email and SHALL NOT show any user's full email address, including the current user's own row.

#### Scenario: Email is masked
- **GIVEN** a user with the email `jan.kowalski@example.com`
- **WHEN** the leaderboard is shown
- **THEN** the row shows a masked form of the address, not the full email

### Requirement: Paginated list
The system SHALL show the leaderboard in pages of 30 users, starting with the first page, ordered by position, with controls to move between pages. Each row SHALL show position, masked email and points, and the current user's row SHALL be visually distinguished.

#### Scenario: First page
- **GIVEN** 70 active users
- **WHEN** a user opens the leaderboard
- **THEN** positions 1 to 30 are shown and the user can go to the next page

#### Scenario: Last page
- **GIVEN** 70 active users
- **WHEN** the user opens the third page
- **THEN** positions 61 to 70 are shown and there is no further page

### Requirement: Motivation banner
The system SHALL show above the list the current user's position and points, the nearest user above them with the point gap, a call to generate flashcards to catch up, and the number of other users with exactly the same points. The banner SHALL NOT depend on the page being viewed.

#### Scenario: Rival ahead
- **GIVEN** the current user has 12 points and the nearest user above has 14
- **WHEN** the leaderboard is shown
- **THEN** the banner names that user by masked email, states a gap of 2 points and urges the user to generate flashcards to catch up

#### Scenario: Others with the same points
- **GIVEN** the current user has 12 points and two other users also have 12
- **WHEN** the leaderboard is shown
- **THEN** the banner says the user has 2 rivals with the same number of points

#### Scenario: Equal points rival above
- **GIVEN** the nearest user above the current user has the same points because they reached the score first
- **WHEN** the leaderboard is shown
- **THEN** the banner says to create one more flashcard to overtake that user

#### Scenario: Current user is the leader
- **GIVEN** the current user is at position 1
- **WHEN** the leaderboard is shown
- **THEN** the banner says the user leads the ranking and names no rival above
