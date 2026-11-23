---
title: <Entity or data area name>
type: data-model
summary: <One sentence: which entities this page describes and what they store.>
tags: [<keyword>, <synonym>]
sources:
  - <path to entity>
  - <path to EF configuration>
  - <path to migration>
verified_commit: <40-char commit SHA>
confidence: high
---

# <Entity or data area name>

## Entity

<What the entity represents in domain terms.>

## Fields

| Field | Type | Constraints | Meaning |
|---|---|---|---|
| `<name>` | `<type>` | <required, max length, unique> | <meaning> |

## Relations

- <Relation to other entities, with delete behavior.> (`<path>` `<symbol>`)

## Lifecycle

<How records are created, changed and removed, and by which feature.> See [[<feature-page>]].

## Discrepancies

<Only when sources disagree. Delete this section otherwise.>

## Related

- [[<other-page>]]
