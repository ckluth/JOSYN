# Session 0002 — JOSYN.System Namespace Decision

## Context

The open question from session-0001 (Key Decision #4 / Open Question #1):  
*"What is the correct name/namespace for the JOSYN contract layer?"*

The opener proposed introducing a `JOSYN.System` grouping layer as a deliberate
architectural statement, not just a technical namespace.

## Discussion Summary

### Flat vs. Grouping
Evaluated both options. Grouping only earns its place if the middle segment carries
genuine semantic weight. The relationship between Frontend, Backend, and Contract
is strong enough — they are all parts of *the system being built*.

### Naming Candidates
`JOSYN.Application` rejected — too generic, doesn't feel right.  
Brainstorming produced no stronger alternative.  
`JOSYN.System` conflict with .NET `System` namespace assessed as **aesthetic only** —
no compile errors, no runtime issues, minor reader hesitation in rare edge cases only.

## Decision

**`JOSYN.System` is the accepted grouping layer.**

```
JOSYN.Foundation.*          ← stable base, no JOSYN semantics
JOSYN.System.Frontend       ← evolving application frontend
JOSYN.System.Backend        ← evolving application backend
JOSYN.System.Contract       ← single artifact: IJosynApplicationProtocol.cs only
```

`JOSYN.System.Contract` contains exactly one file: `IJosynApplicationProtocol.cs`.  
Never more, never less. It is the shared, evolving contract between Frontend and Backend.

## Status

Open Question #1 from `_index.md` is resolved.  
No code changes or commits in this session — decision only. 
