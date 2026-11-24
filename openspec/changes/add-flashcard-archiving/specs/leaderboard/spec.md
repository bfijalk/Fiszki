## RENAMED Requirements

- FROM: `### Requirement: Points equal the user's flashcards`
- TO: `### Requirement: Points for flashcards and archived flashcards`

## MODIFIED Requirements

### Requirement: Points for flashcards and archived flashcards
The system SHALL give each user one point per flashcard they currently own, counting AI-generated and
manually created flashcards equally, plus 3 extra points for each of those flashcards that is
archived (marked as learned). An archived flashcard is therefore worth 4 points. Points SHALL always
follow the current state: restoring an archived flashcard removes its 3 extra points and deleting a
flashcard, archived or not, removes all the points it gave.

#### Scenario: Both creation sources count
- **GIVEN** a user owns 3 AI-generated and 2 manually created flashcards and none is archived
- **WHEN** the leaderboard is computed
- **THEN** the user has 5 points

#### Scenario: Archiving adds 3 points
- **GIVEN** a user with 5 flashcards and 5 points
- **WHEN** the user archives one flashcard and the leaderboard is shown again
- **THEN** the user has 8 points, 5 flashcards and 1 archived flashcard

#### Scenario: Restoring removes the extra points
- **GIVEN** a user with 5 flashcards, 2 archived, and 11 points
- **WHEN** the user restores one archived flashcard and the leaderboard is shown again
- **THEN** the user has 8 points and 1 archived flashcard

#### Scenario: Deleting a flashcard removes a point
- **GIVEN** a user with 5 flashcards, none archived, and 5 points
- **WHEN** the user deletes one flashcard and the leaderboard is shown again
- **THEN** the user has 4 points

#### Scenario: Deleting an archived flashcard removes 4 points
- **GIVEN** a user with 5 flashcards, 1 archived, and 8 points
- **WHEN** the user deletes the archived flashcard and the leaderboard is shown again
- **THEN** the user has 4 points, 4 flashcards and 0 archived flashcards

### Requirement: Ranking order with unique positions
The system SHALL order users by points from highest to lowest and SHALL assign each user a unique
position (1, 2, 3, ...). Users with equal points SHALL be ordered by who reached the score first,
that is, the user whose latest score-changing event is earlier ranks higher. The score-changing
events are creating a flashcard and archiving one. The learning progress percentage SHALL NOT affect
the order.

#### Scenario: Higher points rank higher
- **GIVEN** user A has 10 points and user B has 7 points
- **WHEN** the leaderboard is shown
- **THEN** A is above B

#### Scenario: Tie broken by who reached the score first
- **GIVEN** users A and B both have 7 points and A's latest flashcard creation or archiving is older than B's
- **WHEN** the leaderboard is shown
- **THEN** A has the better position and the two positions are different

#### Scenario: Archiving counts as reaching the score
- **GIVEN** users A and B both have 8 points, A created all flashcards earlier but archived one after B archived theirs
- **WHEN** the leaderboard is shown
- **THEN** B has the better position

#### Scenario: Learning progress does not change the order
- **GIVEN** user A has 8 points with 20% learning progress and user B has 9 points with 5% learning progress
- **WHEN** the leaderboard is shown
- **THEN** B is above A

### Requirement: Paginated list
The system SHALL show the leaderboard in pages of 30 users, starting with the first page, ordered by
position, with controls to move between pages. Each row SHALL show position, masked email, the number
of flashcards, the number of archived flashcards, the points and the learning progress percentage of
that user, and the current user's row SHALL be visually distinguished. The learning progress SHALL be
shown for information and SHALL NOT be presented as part of the points.

#### Scenario: First page
- **GIVEN** 70 active users
- **WHEN** a user opens the leaderboard
- **THEN** positions 1 to 30 are shown and the user can go to the next page

#### Scenario: Last page
- **GIVEN** 70 active users
- **WHEN** the user opens the third page
- **THEN** positions 61 to 70 are shown and there is no further page

#### Scenario: Row shows both numbers and the progress
- **GIVEN** a user with 4 flashcards, 1 of them archived
- **WHEN** the leaderboard is shown
- **THEN** the row shows 4 flashcards, 1 archived, 7 points and 25% learning progress

### Requirement: Motivation banner
The system SHALL show above the list the current user's position, points, number of flashcards,
number of archived flashcards and learning progress, the nearest user above them with the point gap,
a call to create or learn flashcards to catch up, and the number of other users with exactly the same
points. The banner SHALL NOT depend on the page being viewed.

#### Scenario: Rival ahead
- **GIVEN** the current user has 12 points and the nearest user above has 14
- **WHEN** the leaderboard is shown
- **THEN** the banner names that user by masked email, states a gap of 2 points and urges the user to create or learn flashcards to catch up

#### Scenario: Others with the same points
- **GIVEN** the current user has 12 points and two other users also have 12
- **WHEN** the leaderboard is shown
- **THEN** the banner says the user has 2 rivals with the same number of points

#### Scenario: Equal points rival above
- **GIVEN** the nearest user above the current user has the same points because they reached the score first
- **WHEN** the leaderboard is shown
- **THEN** the banner says to create or learn one more flashcard to overtake that user

#### Scenario: Current user is the leader
- **GIVEN** the current user is at position 1
- **WHEN** the leaderboard is shown
- **THEN** the banner says the user leads the ranking and names no rival above

#### Scenario: Banner shows the user's own numbers
- **GIVEN** the current user has 4 flashcards, 1 archived
- **WHEN** the leaderboard is shown
- **THEN** the banner shows 7 points, 4 flashcards, 1 archived and 25% learning progress
