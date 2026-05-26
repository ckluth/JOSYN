# Session Results Method — Process Refinement

> **Story:** meta  
> **Type:** generation  
> **Date:** 2026-05-26  
> **Opener:** session-0004-opener.md

---

## What Happened

This session refined the session-results process in two areas: the opener format and the human-facing documentation.

---

## Part 1 — Improved Opener Format

### Problem

The existing opener format was a loose suggestion (topic, short-description, session content, desired artifacts, session result). Actual openers diverged freely from it, and it contained redundant boilerplate (e.g. "read the whole opener" — which the AI always does).

### New Format

Five sections; **Constraints** and **Expected Artifacts** are optional and should be omitted entirely when not needed:

| Section | Purpose |
|---|---|
| `## Meta` | Machine-readable identity: Story, Session number, Short description |
| `## Background` | 1–3 sentences of context — the "why" for this session |
| `## Goals` | Numbered, verifiable — what "done" looks like |
| `## Constraints` | Guard rails: output paths, naming, language, things to avoid *(optional)* |
| `## Expected Artifacts` | Exit checklist: files to produce with path + description *(optional)* |

### Opener Template

A blank, annotated template was created at:  
`.github\.artifacts\session-opener-template.md`

---

## Part 2 — Artifacts Generated

### AI artifact — `copilot-instructions.md` updated

The `## Session opener` section was updated to:
- Describe the new five-section format inline (compact reference block)
- Add a pointer to the template file

### Human artifact — new canonical user documentation

`.github\.artifacts\session-results-process.md` created — supersedes `session-0002-session-results-method-description.md`.

Contents:
- The problem this solves
- Core concept (stories, sessions)
- Session file naming + types
- Story index (`_index.md`) — purpose, contents, rules
- Session openers — when to use, format, template, AI behavior
- Save trigger, archiving, directory layout
- Rules at a glance
- Copy-paste setup block for new repositories

---

## Artifacts Produced

| File | Action |
|---|---|
| `.github\.artifacts\session-opener-template.md` | Created — blank annotated opener template |
| `.github\.artifacts\session-results-process.md` | Created — canonical human-readable process documentation |
| `.github\copilot-instructions.md` | Updated — opener section: new format + template reference |
| `meta\session-0004-process-refinement-generation.md` | This file |
