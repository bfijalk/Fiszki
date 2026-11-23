#!/usr/bin/env bash
# Behavior tests for wiki/tools (search now, lint added with the lint tool).
# Requires: bash, jq, dotnet 10 SDK. Run from anywhere: wiki/tools/tests/run-tests.sh
set -u

HERE="$(cd "$(dirname "$0")" && pwd)"
TOOLS="$(cd "$HERE/.." && pwd)"
MINI="$HERE/mini-wiki"
SEARCH="$TOOLS/search"

command -v jq >/dev/null || { echo "jq is required"; exit 2; }

PASS=0
FAIL=0
ok()   { PASS=$((PASS + 1)); echo "  ok   - $1"; }
fail() { FAIL=$((FAIL + 1)); echo "  FAIL - $1"; [ -n "${2:-}" ] && echo "         $2"; }

# assert_eq <name> <expected> <actual>
assert_eq() { if [ "$2" = "$3" ]; then ok "$1"; else fail "$1" "expected [$2] got [$3]"; fi; }

echo "search"

# --- title/tags/summary matches rank above body-only matches -------------------------
paths="$("$SEARCH" deletion --root "$MINI" --json | jq -r '.[].path | split("/") | last')"
assert_eq "title match ranks first" "flashcard-deletion.md" "$(echo "$paths" | head -1)"
assert_eq "body-only matches are still returned" "3" "$(echo "$paths" | wc -l | tr -d ' ')"

# --- result fields -------------------------------------------------------------------
fields="$("$SEARCH" deletion --root "$MINI" --json --limit 1 | jq -r '.[0] | keys | join(",")')"
assert_eq "json result fields" "path,score,snippet,summary,title,type" "$fields"
assert_eq "score is a number" "number" "$("$SEARCH" deletion --root "$MINI" --json --limit 1 | jq -r '.[0].score | type')"

# --- filtering -----------------------------------------------------------------------
types="$("$SEARCH" flashcard --root "$MINI" --type business-rule --json | jq -r '.[].type' | sort -u)"
assert_eq "type filter returns only that type" "business-rule" "$types"
assert_eq "type filter excludes other types" "0" "$("$SEARCH" deletion --root "$MINI" --type architecture --json | jq 'length')"
assert_eq "tag filter" "flashcard-validation.md" \
  "$("$SEARCH" flashcard --root "$MINI" --tag validation --json | jq -r '.[].path | split("/") | last')"
assert_eq "limit" "2" "$("$SEARCH" flashcard --root "$MINI" --limit 2 --json | jq 'length')"

# --- non-pages are never returned ----------------------------------------------------
all="$("$SEARCH" deletion --root "$MINI" --json --limit 50 | jq -r '.[].path')"
for forbidden in "/index.md" "/log.md" "/SCHEMA.md" "templates/" "draft-notes"; do
  if echo "$all" | grep -q "$forbidden"; then fail "never returns $forbidden"; else ok "never returns $forbidden"; fi
done

# --- empty result is explicit and not an error ---------------------------------------
out="$("$SEARCH" zebra --root "$MINI")"; code=$?
assert_eq "no match: stdout is empty" "" "$out"
assert_eq "no match: exit code 0" "0" "$code"
assert_eq "no match: json is an empty array" "0" "$("$SEARCH" zebra --root "$MINI" --json | jq 'length')"

# --- multi-term natural-language query uses stop words correctly ---------------------
"$SEARCH" "how does the" --root "$MINI" >/dev/null 2>&1; assert_eq "query of only stop words is a usage error (exit 2)" "2" "$?"
# "How" appears in a fixture summary; as a stop word it must not make that page match.
assert_eq "stop words do not match unrelated pages" "0" "$("$SEARCH" "how zebra" --root "$MINI" --json | jq 'length')"
assert_eq "natural language query finds the feature" "flashcard-deletion.md" \
  "$("$SEARCH" "how does flashcard deletion work" --root "$MINI" --json | jq -r '.[0].path | split("/") | last')"

# --- output does not depend on locale ------------------------------------------------
if LC_ALL=pl_PL.UTF-8 "$SEARCH" deletion --root "$MINI" --limit 1 | grep -Eq 'score [0-9]+\.[0-9]{4}$'; then
  ok "text score uses a decimal point"; else fail "text score uses a decimal point"; fi

# --- usage errors --------------------------------------------------------------------
"$SEARCH" --root "$MINI" >/dev/null 2>&1; assert_eq "missing query is a usage error (exit 2)" "2" "$?"
"$SEARCH" deletion --bogus >/dev/null 2>&1; assert_eq "unknown option is a usage error (exit 2)" "2" "$?"

# =====================================================================================
# lint
# Every test copies the clean fixture repo (tests/lint-clean) into a temp directory,
# turns it into a real git repository, applies ONE mutation and checks the findings.
# =====================================================================================
LINT="$TOOLS/lint"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

# new_repo -> path of a fresh git repo whose wiki lints clean. Pages are verified against c1.
new_repo() {
  local t; t="$(mktemp -d "$WORK/repo.XXXXXX")"
  cp -R "$HERE/lint-clean/." "$t/"
  find "$t" -name '*.fixture' | while read -r f; do mv "$f" "${f%.fixture}"; done
  (
    cd "$t" || exit 1
    git init -q
    git config user.email test@example.com
    git config user.name test
    git config commit.gpgsign false
    git add -A && git commit -q -m c1
    sha="$(git rev-parse HEAD)"
    find wiki -name '*.md' -exec perl -pi -e "s/__COMMIT__/$sha/g" {} +
    git add -A && git commit -q -m c2
  ) >/dev/null 2>&1
  echo "$t"
}

lint_json()  { "$LINT" --root "$1/wiki" --json; }
has_code()   { lint_json "$1" | jq -e --arg c "$2" '.findings | any(.code == $c)' >/dev/null; }
codes()      { lint_json "$1" | jq -r '[.findings[].code] | unique | join(",")'; }
lint_exit()  { "$LINT" --root "$1/wiki" >/dev/null 2>&1; echo $?; }
# expect_code <name> <repo> <code>
expect_code() { if has_code "$2" "$3"; then ok "$1"; else fail "$1" "wanted code '$3', got [$(codes "$2")]"; fi; }
# write_page <repo> <relative path> <type> [extra body]
write_page() {
  local slug; slug="$(basename "$2" .md)"
  mkdir -p "$(dirname "$1/$2")"
  printf -- '---\ntitle: %s\ntype: %s\nsummary: A page.\ntags: [x]\nsources: [Fiszki.Database/Entities/Flashcard.cs]\nverified_commit: %s\nconfidence: high\n---\n\n# %s\n\n%s\n' \
    "$slug" "$3" "$(git -C "$1" rev-parse HEAD~1)" "$slug" "${4:-}" > "$1/$2"
}

echo
echo "lint: structure (3.1)"

R="$(new_repo)"
assert_eq "clean wiki has no findings" "0" "$(lint_json "$R" | jq '.findings | length')"
assert_eq "clean wiki exits 0" "0" "$(lint_exit "$R")"

R="$(new_repo)"; echo 'See [[nope]].' >> "$R/wiki/features/flashcard-deletion.md"
expect_code "dead [[link]] is reported" "$R" dead-link
assert_eq "dead link makes lint exit 1" "1" "$(lint_exit "$R")"

R="$(new_repo)"; echo 'Not links: `[[ignored]]`' >> "$R/wiki/features/flashcard-deletion.md"
if has_code "$R" dead-link; then fail "links inside inline code are ignored"; else ok "links inside inline code are ignored"; fi

R="$(new_repo)"; write_page "$R" wiki/features/new-page.md feature $'## Business rules\n\n## Implementation'
expect_code "page missing from index.md is reported" "$R" not-in-index

R="$(new_repo)"; write_page "$R" wiki/features/lonely.md feature $'## Business rules\n\n## Implementation'
echo '- [[lonely]] - nobody links here' >> "$R/wiki/index.md"
expect_code "orphan page is reported" "$R" orphan
assert_eq "orphan is only a warning (exit 0)" "0" "$(lint_exit "$R")"
assert_eq "orphan severity" "warning" "$(lint_json "$R" | jq -r '.findings[] | select(.code=="orphan") | .severity')"

R="$(new_repo)"; perl -ni -e 'print unless /^summary:/' "$R/wiki/data-model/flashcard-entity.md"
expect_code "missing frontmatter field is reported" "$R" frontmatter-field

R="$(new_repo)"; perl -pi -e 's/^type: data-model/type: banana/' "$R/wiki/data-model/flashcard-entity.md"
expect_code "invalid type is reported" "$R" frontmatter-field

R="$(new_repo)"; perl -pi -e 's/^verified_commit: .*/verified_commit: abc123/' "$R/wiki/data-model/flashcard-entity.md"
expect_code "malformed verified_commit is reported" "$R" frontmatter-field

R="$(new_repo)"; printf '# No frontmatter\n\nJust text.\n' > "$R/wiki/features/plain.md"; echo '- [[plain]] - x' >> "$R/wiki/index.md"
expect_code "page without frontmatter is reported" "$R" frontmatter-missing

R="$(new_repo)"; perl -pi -e 's/^## Implementation/## Impl/' "$R/wiki/features/flashcard-deletion.md"
expect_code "feature page without required sections is reported" "$R" feature-sections

R="$(new_repo)"; write_page "$R" wiki/business-rules/flashcard-entity.md business-rule
expect_code "duplicate page names are reported" "$R" duplicate-slug

R="$(new_repo)"; rm "$R/wiki/index.md"
expect_code "missing index.md is reported" "$R" index-missing

R="$(new_repo)"; printf '\n## Discrepancies\n\nCode says X, spec says Y.\n' >> "$R/wiki/data-model/flashcard-entity.md"
expect_code "open Discrepancies section is reported" "$R" discrepancy-open

echo
echo "lint: code references and drift (3.2)"

R="$(new_repo)"; echo 'See `Fiszki.Services/Services/Missing.cs`.' >> "$R/wiki/features/flashcard-deletion.md"
expect_code "cited file that does not exist" "$R" code-ref-path

R="$(new_repo)"; echo 'See `Fiszki.Services/Services/FlashcardService.cs` `NoSuchMethod`.' >> "$R/wiki/features/flashcard-deletion.md"
expect_code "cited symbol that is gone (renamed class/method)" "$R" code-ref-symbol

R="$(new_repo)"; perl -pi -e 's/Delete a flashcard with confirmation/Nonexistent scenario/' "$R/wiki/features/flashcard-deletion.md"
expect_code "cited scenario that does not exist" "$R" scenario-missing

R="$(new_repo)"; perl -pi -e 's/Owner can delete a flashcard/Nonexistent requirement/' "$R/wiki/features/flashcard-deletion.md"
expect_code "cited OpenSpec requirement that does not exist" "$R" requirement-missing

R="$(new_repo)"; perl -pi -e 's/^sources:$/sources:\n  - Does\/Not\/Exist.cs/' "$R/wiki/features/flashcard-deletion.md"
expect_code "source path that does not exist" "$R" source-missing

R="$(new_repo)"; perl -pi -e 's/^sources:$/sources:\n  - .ai\/prd.md/' "$R/wiki/features/flashcard-deletion.md"
expect_code ".ai/ as a source is rejected" "$R" source-forbidden

R="$(new_repo)"; perl -pi -e 's/^verified_commit: .*/verified_commit: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/' "$R/wiki/data-model/flashcard-entity.md"
expect_code "verified_commit that is not in the repository" "$R" commit-unknown

# Drift: a changed source after verified_commit marks exactly the pages that list it.
R="$(new_repo)"
echo '// changed' >> "$R/Fiszki.Services/Services/FlashcardService.cs"
git -C "$R" add -A && git -C "$R" commit -q -m "change service"
expect_code "changed source marks the page stale" "$R" stale
stale_pages="$(lint_json "$R" | jq -r '[.findings[] | select(.code=="stale") | .path | split("/") | last] | join(",")')"
assert_eq "only the page listing the changed file is stale" "flashcard-deletion.md" "$stale_pages"
assert_eq "stale message names the changed file" "true" \
  "$(lint_json "$R" | jq -r '[.findings[] | select(.code=="stale")][0].message | contains("FlashcardService.cs")')"
assert_eq "stale is only a warning (exit 0)" "0" "$(lint_exit "$R")"

# Re-verifying the page against the new HEAD clears the staleness.
head_sha="$(git -C "$R" rev-parse HEAD)"
perl -pi -e "s/^verified_commit: .*/verified_commit: $head_sha/" "$R/wiki/features/flashcard-deletion.md"
if has_code "$R" stale; then fail "updating verified_commit clears staleness"; else ok "updating verified_commit clears staleness"; fi

R="$(new_repo)"; echo 'text' > "$R/README.md"; git -C "$R" add -A && git -C "$R" commit -q -m "unrelated"
if has_code "$R" stale; then fail "unrelated file change does not mark pages stale"; else ok "unrelated file change does not mark pages stale"; fi

R="$(mktemp -d "$WORK/nogit.XXXXXX")"; cp -R "$HERE/lint-clean/." "$R/"
find "$R" -name '*.fixture' | while read -r f; do mv "$f" "${f%.fixture}"; done
find "$R/wiki" -name '*.md' -exec perl -pi -e 's/__COMMIT__/0000000000000000000000000000000000000000/g' {} +
expect_code "outside a git repo: drift check is skipped with a warning" "$R" no-git

echo
echo "lint: log and read-only (3.3)"

R="$(new_repo)"; echo '## [bad-date] ingest | x' >> "$R/wiki/log.md"
expect_code "malformed log entry" "$R" log-format

R="$(new_repo)"; printf '\n## [2026-02-02] ingest | Added a feature\n' >> "$R/wiki/log.md"
if has_code "$R" log-edited || has_code "$R" log-format; then fail "appending a valid log entry is accepted"; else ok "appending a valid log entry is accepted"; fi

R="$(new_repo)"; perl -pi -e 's/Fixture created/Rewritten history/' "$R/wiki/log.md"
expect_code "editing an existing log entry" "$R" log-edited

# Read-only: lint on a repo with many findings must not change a single byte, nor git's state.
R="$(new_repo)"
echo 'See [[nope]] and `Fiszki.Services/Services/Missing.cs`.' >> "$R/wiki/features/flashcard-deletion.md"
echo '// changed' >> "$R/Fiszki.Services/Services/FlashcardService.cs"
git -C "$R" add -A && git -C "$R" commit -q -m "change"
tree_hash() { (cd "$1" && find . -type f -not -path './.git/*' -exec shasum {} + | sort | shasum | cut -d' ' -f1); }
state_hash() { (cd "$1" && { git status --porcelain; git rev-parse HEAD; shasum .git/index; } | shasum | cut -d' ' -f1); }
before_tree="$(tree_hash "$R")"; before_state="$(state_hash "$R")"
lint_json "$R" >/dev/null; "$LINT" --root "$R/wiki" >/dev/null
assert_eq "lint leaves every file unchanged" "$before_tree" "$(tree_hash "$R")"
assert_eq "lint leaves git status, HEAD and index unchanged" "$before_state" "$(state_hash "$R")"
assert_eq "read-only run still reported errors" "1" "$(lint_exit "$R")"

echo
echo "passed: $PASS, failed: $FAIL"
[ "$FAIL" -eq 0 ]
