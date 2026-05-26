# Story: meta

## Key Decisions

- **Terminology update** (session-0002) — "topic" → "story", "case" → "chapter", "brief-slug" → "short-description"; "session" and "archive" unchanged
- **Opener file type introduced** (session-0002) — optional `session-NNNN-opener[-short-description].md`; user places it before a session; AI reads, paraphrases, asks for clarification, then works
- **`_index.md` introduced** (session-0003) — AI-maintained living summary per story; read at session start for instant context; created on first save, updated on every subsequent save; never archived
- **Opener format standardized** (session-0004) — five sections: Meta, Background, Goals, Constraints (optional), Expected Artifacts (optional); template at `.github\.artifacts\session-opener-template.md`; canonical user doc at `.github\.artifacts\story-method.md`

## Open Questions

*(none currently open)*

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-session-process-description.md | Original human-readable description of the session results method |
| 0002 | session-0002-opener-session-result-method-enhancement.md | Opener for this story's enhancement session: rename concepts + introduce opener file type |
| 0002 | session-0002-session-results-method-description.md | Updated method description: new terminology, opener concept, _index.md concept |
| 0003 | session-0003-enhancements-applied-generation.md | Executed opener: all renames, instructions update, session-0002 created; _index.md concept implemented and all three index files created |
| 0004 | session-0004-process-refinement-generation.md | Improved opener format (5 sections, optional tail); opener template created; canonical human doc written to .artifacts |
