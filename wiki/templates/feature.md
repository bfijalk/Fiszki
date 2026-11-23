---
title: <Feature name>
type: feature
summary: <One sentence: what this feature does and when to read this page.>
tags: [<keyword>, <synonym>]
sources:
  - <path to .feature file>
  - <path to service / component / entity>
verified_commit: <40-char commit SHA>
confidence: high
---

# <Feature name>

## Summary

<What the feature does for the user, in two or three sentences.>

## Business rules

For analysts. One rule per bullet, each citing its source.

- <Rule.> (`<path to .feature>`, scenario "<scenario name>")
- <Rule.> (`openspec/specs/<capability>/spec.md`, requirement "<name>")

## User flow

1. <Step the user takes.>
2. <What the system does.>

## Implementation

For developers. Cite file path and symbol.

- **UI:** `<path>` `<component>`
- **Service:** `<path>` `<method>`
- **Validation:** `<path>` `<validator>`
- **Data:** `<path>` `<entity>`
- **Tests:** `<path>`

## Dependencies and side effects

- <Other features, external services, data touched.>

## Discrepancies

<Only when sources disagree: both claims, both sources, date. Delete this section otherwise.>

## Related

- [[<other-page>]]
