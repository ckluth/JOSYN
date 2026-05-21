# The Session Results Method
### A process for managing AI-assisted work across multiple sessions

---

## The Problem This Solves

When working with an AI coding assistant (like GitHub Copilot CLI) on an evolving project, context gets lost between sessions. You end up re-explaining decisions, re-researching conclusions, and re-establishing shared understanding every time you start fresh. This method gives you a lightweight, git-native way to accumulate and preserve the outputs of AI sessions — without bureaucratic overhead.

---

## Core Concept

All session outputs live under `.github\session-results\` in your repository. This folder is part of your git history, so context survives terminal restarts, machine switches, and time.

The structure has two levels:
1. **Topic directory** — a named subject area (e.g. `result-pattern\`, `ipc\`, `meta\`)
2. **Session files** — one file per meaningful session, accumulated over time

There is no setup step. You don't "open" a topic formally. You just create a file in a topic folder and you're working.

---

## Session File Naming

```
session-NNNN-[brief-slug]-[type].md
```

| Part | Description |
|---|---|
| `NNNN` | 4-digit zero-padded sequence number, **continuous per topic** (never resets) |
| `brief-slug` | 2–4 word kebab-case hint of the content |
| `type` | What kind of output this is (see below) |

**Valid types:**

| Type | When to use |
|---|---|
| `discussion` | Back-and-forth exploration, no firm conclusion yet |
| `summary` | Condensed record of what was decided or produced |
| `conclusion` | Final answer / decision on a question |
| `analysis` | Deep investigation of a specific topic |
| `generation` | Session primarily produced an artifact (code, doc, config) |

**The directory already carries topic context — never repeat it in the filename.**

✅ `result-pattern\session-0001-make-or-buy-summary.md`  
❌ `result-pattern\result-pattern-session-0001-make-or-buy-summary.md`

**Examples:**
```
result-pattern\session-0001-make-or-buy-summary.md
result-pattern\session-0002-railway-operators-discussion.md
ipc\session-0001-async-handler-analysis.md
ipc\session-0002-protocol-redesign-conclusion.md
```

---

## How to Trigger a Save

Say something like: *"save this session"*, *"write a summary"*, *"log this"*, *"write a conclusion"*.

The AI will **propose a filename** and wait for your confirmation before writing anything. You can accept the suggestion or correct it.

---

## Archiving

Sessions accumulate in the topic root over time. When a batch of sessions forms a natural unit (a milestone, an iteration, a decision cycle) and you're ready to close it:

**Say:** *"archive the current case"*  
**Optionally:** *"archive the current case as first-iteration"*

The AI will:
1. Move all session files currently in the topic root into:  
   `archives\archive-NNN\` or `archives\archive-NNN-optional-name\`  
   (3-digit counter, increments per topic)
2. Session numbering in the topic **continues from where it left off** — no reset.
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
  result-pattern\                                  ← active topic
    session-0001-make-or-buy-summary.md
    session-0002-railway-operators-discussion.md
    archives\
      archive-001-initial-evaluation\              ← sealed
        session-0001-...md                         ← (hypothetical earlier batch)
        conclusion.md                              ← optional

  ipc\                                             ← active topic
    session-0005-reconnect-logic-analysis.md       ← numbering continues after archives
    archives\
      archive-001-first-stable-poc\                ← sealed
        session-0001-...md
        session-0004-...md
      archive-002-stable-poc-v2\                   ← sealed
        ...

  meta\                                            ← active topic
    session-0001-session-process-description.md   ← this file
```

---

## Rules at a Glance

| Rule | Detail |
|---|---|
| Never overwrite | Each session appends a new file |
| Never reset numbering | 4-digit counter per topic, continuous across archives |
| Never modify archives | Sealed after creation |
| Filename never repeats folder | Directory = topic context |
| AI always proposes, you confirm | No file is written without your approval |
| Conclusion is optional | Only on explicit request; negotiated on the fly |

---

## Setting This Up for Your Repository

Add the following to your `.github\copilot-instructions.md` (or `AGENTS.md`):

```
Session results are stored under `.github\session-results\<topic>\`.
Session file naming: session-NNNN-[brief-slug]-[type].md
(NNNN = 4-digit zero-padded, continuous per topic; type = discussion|summary|conclusion|analysis|generation)
When user says "save", "summarize", "log this" etc: propose a filename, wait for confirmation, then write.
Archiving: "archive the current case [as <name>]" → move session files to archives\archive-NNN[-name]\, offer conclusion if 3+ sessions.
```

---

*This document was itself produced using the method it describes.*
