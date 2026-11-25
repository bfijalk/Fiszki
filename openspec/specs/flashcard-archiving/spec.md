# flashcard-archiving Specification

## Purpose

Lets a user mark a flashcard as learned (archive it) after a confirmation, keep archived flashcards in a separate view where they can be restored or deleted, and see a learning progress percentage that shows how many of their flashcards they have already learned.

## Requirements

### Requirement: "I learned it" button
The system SHALL show an "I learned it" button on every active flashcard on the Flashcards page, in
both the card view and the list view, next to the existing Edit and Delete buttons. Clicking it SHALL
only open a confirmation dialog and SHALL NOT archive the flashcard by itself.

#### Scenario: Button is available on each active flashcard
- **GIVEN** a logged-in user with active flashcards
- **WHEN** the user opens the Flashcards page in the card view or the list view
- **THEN** every flashcard has an "I learned it" button in addition to Edit and Delete

#### Scenario: Clicking the button does not archive
- **WHEN** the user clicks "I learned it" on a flashcard
- **THEN** a confirmation dialog is shown and the flashcard is still in the active list

### Requirement: Archiving needs confirmation
The system SHALL ask the user to confirm before marking a flashcard as learned. The dialog SHALL show
the question and the answer of the selected flashcard and ask whether the user is sure they want to
mark it as learned. Confirming SHALL archive the flashcard; cancelling or closing the dialog SHALL
leave the flashcard unchanged.

#### Scenario: Confirm archives the flashcard
- **GIVEN** the confirmation dialog is open for a flashcard
- **WHEN** the user confirms
- **THEN** the dialog closes, the flashcard disappears from the active list and appears in the archived view

#### Scenario: Cancel keeps the flashcard active
- **GIVEN** the confirmation dialog is open for a flashcard
- **WHEN** the user cancels or closes the dialog
- **THEN** the flashcard stays in the active list and its learning progress does not change

### Requirement: Archived flashcards leave the active list
The system SHALL NOT show an archived flashcard in the active (main) list of the Flashcards page, and
SHALL show it in a separate archived view that the user can switch to from the same page. Archiving
SHALL NOT delete or change the content of the flashcard.

#### Scenario: Archived flashcard is only in the archived view
- **GIVEN** a user archived a flashcard
- **WHEN** the user looks at the active list and then switches to the archived view
- **THEN** the flashcard is missing from the active list and is shown, with its question and answer, in the archived view

#### Scenario: Empty archived view
- **GIVEN** a user with no archived flashcards
- **WHEN** the user switches to the archived view
- **THEN** the page tells the user that no flashcards are archived yet

### Requirement: Restoring an archived flashcard
The system SHALL let the user restore an archived flashcard from the archived view with a "Restore"
button, without a confirmation dialog. A restored flashcard SHALL return to the active list and
SHALL no longer count as learned.

#### Scenario: Restore returns the flashcard to the active list
- **GIVEN** an archived flashcard
- **WHEN** the user clicks "Restore" in the archived view
- **THEN** the flashcard leaves the archived view and is shown again in the active list

### Requirement: Archived flashcards can be deleted
The system SHALL let the user delete an archived flashcard from the archived view, using the same
confirmation dialog and the same permanent deletion as for an active flashcard. Deleting an archived
flashcard SHALL remove it from the learning progress calculation and from the user's points.

#### Scenario: Delete from the archived view
- **GIVEN** an archived flashcard
- **WHEN** the user clicks Delete in the archived view and confirms
- **THEN** the flashcard is removed permanently and is no longer in any view

### Requirement: Users manage only their own archive
The system SHALL allow a user to archive and restore only flashcards they own. Archiving a flashcard
that is already archived, restoring one that is not archived, or acting on a flashcard that does not
exist or belongs to another user SHALL change nothing and SHALL NOT raise an error.

#### Scenario: Another user's flashcard is not affected
- **GIVEN** a flashcard owned by user A
- **WHEN** user B tries to archive it
- **THEN** the flashcard stays active for user A

#### Scenario: Repeating an action changes nothing
- **GIVEN** an already archived flashcard
- **WHEN** it is archived again
- **THEN** it stays archived and the learning progress does not change a second time

### Requirement: Learning progress percentage
The system SHALL show the user's learning progress on the Flashcards page as the share of the user's
flashcards that are archived, in whole percent, rounded down. All the user's flashcards, active and
archived, AI and manual, SHALL count as 100%. The value SHALL be 0% when the user has no flashcards
and SHALL be 100% only when every flashcard is archived.

#### Scenario: Progress goes up when a flashcard is archived
- **GIVEN** a user with 4 flashcards, none archived (0%)
- **WHEN** the user archives one flashcard
- **THEN** the learning progress shows 25%

#### Scenario: Progress after more archiving
- **GIVEN** a user with 3 flashcards, one of them archived (33%)
- **WHEN** the user archives a second one
- **THEN** the learning progress shows 66%

#### Scenario: Progress goes down on restore
- **GIVEN** a user with 4 flashcards, 2 archived (50%)
- **WHEN** the user restores one
- **THEN** the learning progress shows 25%

#### Scenario: Deleting an archived flashcard changes the base
- **GIVEN** a user with 4 flashcards, 2 archived (50%)
- **WHEN** the user deletes one of the archived flashcards
- **THEN** the learning progress shows 33%

#### Scenario: Progress for a user without flashcards
- **GIVEN** a user with no flashcards
- **WHEN** the user opens the Flashcards page or the leaderboard
- **THEN** the Flashcards page shows its "No flashcards yet" state instead of the progress, and the leaderboard shows 0% for that user

### Requirement: "No flashcards yet" only without any flashcards
The system SHALL show the "No flashcards yet" state on the Flashcards page, instead of the progress,
the counters and the view switch, only when the user has no flashcards at all, archived or not. When the
user has flashcards but the chosen view or filter shows none, the page SHALL keep the progress, the
counters and the view switch and SHALL say that the view has no flashcards.

#### Scenario: All flashcards archived
- **GIVEN** a user whose flashcards are all archived
- **WHEN** the user opens the Flashcards page
- **THEN** the page shows the progress (100%), the counters and the view switch, and the active list says there are no active flashcards

#### Scenario: Archived view with nothing archived
- **GIVEN** a user with active flashcards and none archived
- **WHEN** the user switches to the archived view
- **THEN** the page keeps the progress, the counters and the view switch and says that no flashcards are archived

### Requirement: Counters include archived flashcards
The system SHALL count archived flashcards in the "Total Cards", "AI Generated" and "Manual" counters
on the Flashcards page, so that archiving a flashcard does not change any of them, and "AI Generated"
plus "Manual" SHALL always equal "Total Cards". The page SHALL also show how many flashcards are
archived.

#### Scenario: Archiving does not change the counters
- **GIVEN** a user with 5 flashcards (Total Cards 5)
- **WHEN** the user archives one
- **THEN** Total Cards is still 5, AI Generated plus Manual is still 5 and the archived count is 1

#### Scenario: Deleting lowers the counters
- **GIVEN** a user with 5 flashcards, one of them archived
- **WHEN** the user deletes the archived flashcard
- **THEN** Total Cards is 4 and the archived count is 0
