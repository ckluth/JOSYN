# The Session Results Method
### A process for managing AI-assisted work across multiple sessions

*Updated version — based on `session-0001-session-process-description.md`, revised terminology and opener concept added in `session-0002`, `_index.md` concept added in `session-0004`.*

---

## The Problem This Solves

When working with an AI coding assistant (like GitHub Copilot CLI) on an evolving project, context gets lost between sessions. You end up re-explaining decisions, re-researching conclusions, and re-establishing shared understanding every time you start fresh. This method gives you a lightweight, git-native way to accumulate and preserve the outputs of AI sessions — without bureaucratic overhead.

---

## Core Concept

All session outputs live under `.github\session-results\` in your repository. This folder is part of your git history, so context survives terminal restarts, machine switches, and time.

The structure has two levels:
1. **Story directory** — a named subject area (e.g. `result-pattern\`, `ipc\`, `meta\`)
2. **Session files** — one file per meaningful session, accumulated over time

There is no setup step. You don't "open" a story formally. You just create a file in a story folder and you're working.

---

## Session File Naming

```
session-NNNN-[short-description]-[type].md
```

| Part | Description |
|---|---|
| `NNNN` | 4-digit zero-padded sequence number, **continuous per story** (never resets) |
| `short-description` | 2–4 word kebab-case hint of the content |
| `type` | What kind of output this is (see below) |

**Valid types:**

| Type | When to use |
|---|---|
| `discussion` | Back-and-forth exploration, no firm conclusion yet |
| `summary` | Condensed record of what was decided or produced |
| `conclusion` | Final answer / decision on a question |
| `analysis` | Deep investigation of a specific topic |
| `generation` | Session primarily produced an artifact (code, doc, config) |
| `opener` | Structured prompt prepared by the user before a session starts |

**The directory already carries story context — never repeat it in the filename.**

✅ `result-pattern\session-0001-make-or-buy-summary.md`  
❌ `result-pattern\result-pattern-session-0001-make-or-buy-summary.md`

**Examples:**
```
result-pattern\session-0001-pre-finalization-analysis-discussion.md
result-pattern\session-0004-make-or-buy-summary.md
ipc\session-0001-jip-client-server-discussion.md
ipc\session-0003-poc-assessment-conclusion.md
meta\session-0002-opener-session-result-method-enhancement.md
```

---

## Session Opener (Optional)

If you want to start a session in a focused, structured way, you can prepare an **opener file** beforehand and place it in the relevant story folder.

**Naming:** `session-NNNN-opener[-short-description].md`  
**Example:** `session-0002-opener-session-result-method-enhancement.md`

The AI will:
1. Read the opener at the start of the session
2. Briefly paraphrase it to confirm understanding
3. Ask for clarification if anything is unclear
4. Then begin working

Openers are entirely optional. Sessions without them work exactly as before. The user is responsible for placing the file in the right folder with the right name.

As a suggestion, openers can use this structure:
- `topic` — which story this belongs to
- `short-description` — a brief label
- `session content` — what should happen in the session
- `desired session artifacts` — what outputs are expected
- `session result` — what the saved file should contain

---

## The Story Index (`_index.md`)

Each story folder can contain a `_index.md` file — a living summary of the story maintained by the AI.

**Purpose:** gives the AI full context in a single read at the start of any session, without having to open individual session files. It is the AI's "memory of the story".

**The AI creates `_index.md`** the first time it saves a session file in a story that doesn't have one yet. It **updates `_index.md` automatically** every time a new session file is saved in that story — no trigger needed from you.

**Contents:**

```markdown
# Story: result-pattern

## Key Decisions
- **Keep custom implementation** (session-0004) — FluentResults/ErrorOr don't fit the
  no-exception + caller-chain model

## Open Questions
- Should Result<T> expose .Value with a guard throw, or stay pure-value?

## Sessions
| # | File | Summary |
|---|---|---|
| 0001 | session-0001-pre-finalization-analysis-discussion.md | Found 5 issues pre-finalization |
| 0002 | session-0002-pre-finalization-fixes-discussion.md | Applied all 5 fixes |
| 0003 | session-0003-stabilization-finalization-discussion.md | Tests, docs, repo structure |
| 0004 | session-0004-make-or-buy-summary.md | Evaluated FluentResults etc.; keep custom |
```

**Sections:**

| Section | Content |
|---|---|
| `Key Decisions` | Firm conclusions reached in this story — the things future sessions must not contradict without knowing they exist |
| `Open Questions` | Unresolved threads — the things a future session might pick up |
| `Sessions` | One-line-per-session index — sequence number, filename, one-sentence summary |

**Rules:**
- The `_index.md` is **not a session file** — it has no `session-NNNN` prefix and is never archived
- When a chapter is archived, `_index.md` stays in the story root and carries forward
- The sessions table in `_index.md` reflects **all** sessions, including archived ones (with an `[archived]` tag)
- The AI updates it silently as part of the save operation — no separate instruction needed

**How it changes session startup:**  
When you start a new session in a story, the AI reads `_index.md` first. It immediately knows what was decided, what's still open, and how many sessions have happened. This makes an opener unnecessary for continuation sessions — you only need an opener when you have something specific and structured to set up.

---

## How to Trigger a Save

Say something like: *"save this session"*, *"write a summary"*, *"log this"*, *"write a conclusion"*.

The AI will **propose a filename** and wait for your confirmation before writing anything. You can accept the suggestion or correct it.

---

## Archiving

Sessions accumulate in the story root over time. When a batch of sessions forms a natural unit (a milestone, an iteration, a decision cycle) and you're ready to close it:

**Say:** *"archive the current chapter"*  
**Optionally:** *"archive the current chapter as first-iteration"*

The AI will:
1. Move all session files currently in the story root into:  
   `archives\archive-NNN\` or `archives\archive-NNN-optional-name\`  
   (3-digit counter, increments per story)
2. Session numbering in the story **continues from where it left off** — no reset.
3. If the batch contained 3 or more sessions, the AI will offer (not require): *"Want a brief conclusion file in the archive?"* — if yes, content is negotiated on the fly. No fixed structure.

**Archive folder naming:**
```
archives\archive-001\
archives\archive-002-first-iteration\
archives\archive-003-stable-poc\
```

Archives are **sealed after creation** — never modified.

---

## Directory Layout — Full Example

```
.github\session-results\
  result-pattern\                                              ← active story
    _index.md                                                  ← AI-maintained story index
    session-0001-pre-finalization-analysis-discussion.md
    session-0002-pre-finalization-fixes-discussion.md
    session-0003-stabilization-finalization-discussion.md
    session-0004-make-or-buy-summary.md

  ipc\                                                         ← active story
    _index.md                                                  ← AI-maintained story index
    session-0001-jip-client-server-discussion.md
    session-0002-why-custom-ipc-discussion.md
    session-0003-poc-assessment-conclusion.md
    archives\
      archive-001-first-stable-poc\                            ← sealed
        ...
      archive-002-stable-poc-v2\                               ← sealed
        ...

  meta\                                                        ← active story
    _index.md                                                  ← AI-maintained story index
    session-0001-session-process-description.md               ← original description
    session-0002-opener-session-result-method-enhancement.md  ← opener for this session
    session-0003-session-results-method-description.md        ← this file
```

---

## Rules at a Glance

| Rule | Detail |
|---|---|
| Never overwrite | Each session appends a new file |
| Never reset numbering | 4-digit counter per story, continuous across archives |
| Never modify archives | Sealed after creation |
| Filename never repeats folder | Directory = story context |
| AI always proposes, you confirm | No file is written without your approval |
| Conclusion is optional | Only on explicit request; negotiated on the fly |
| Opener is optional | User prepares it; AI reads, paraphrases, then works |
| `_index.md` is maintained by AI | Created on first save, updated on every subsequent save |
| `_index.md` is never archived | Stays in story root, carries forward across chapters |

---

## Setting This Up for Your Repository

Add the following to your `.github\copilot-instructions.md` (or `AGENTS.md`):

```
Session results are stored under `.github\session-results\<story>\`.
Session file naming: session-NNNN-[short-description]-[type].md
(NNNN = 4-digit zero-padded, continuous per story; type = discussion|summary|conclusion|analysis|generation|opener)
Opener: if a session-NNNN-opener[-short-description].md exists at session start, read it first, paraphrase briefly, ask for clarification if needed, then proceed.
Story index: each story folder has a _index.md maintained by the AI. Read it at session start for instant context. Create it on first save if absent; update it on every subsequent save (Key Decisions, Open Questions, Sessions table).
Archiving: "archive the current chapter [as <name>]" → move session files to archives\archive-NNN[-name]\, offer conclusion if 3+ sessions. _index.md stays in story root.
```

---

## Closing Reflection

Most "AI memory" solutions are either black-box embeddings the human can't inspect or read, or heavyweight external systems. What we built is:

- **fully transparent** — it's just files you can read, edit, move, diff, and version in git
- **human-readable first** — the human is never locked out of their own context
- **AI-maintainable** — the AI updates it as a side effect of normal work, not as a separate system
- **zero infrastructure** — no database, no API, no sync service

The interesting idea underneath it: *the file system as shared working memory between human and AI, with agreed conventions on structure.* That's not new in principle — but applying it deliberately as an interface design, with the AI as an active participant in maintaining it, is a useful crystallization.

The method is just opinionated enough to be useful and just simple enough to actually survive contact with reality.


*This document was itself produced using the method it describes.*
