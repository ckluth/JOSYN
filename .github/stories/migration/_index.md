# Story: migration

## Working Principles for This Story

> ⚠️ **This is a high-stakes migration. Mistakes made here are costly to correct later.**

- **Slow and thorough** — no rushing, no bundling steps that were agreed to be separate
- **Small, clearly separated steps** — one thing at a time; each step fully visible and reviewable before the next begins
- **Consolidation breaks** — after each meaningful unit of work, stop and present results to the human for inspection before proceeding
- **Human pace** — the collaborator reviews carefully; wait for explicit approval before moving to the next phase
- **No autonomous continuation** — "go for it" on step N is not approval for step N+1
- **When in doubt: stop and ask** — never assume scope, never bundle, never rush to `task_complete`


- **New repo is `JOSYN.POC`** — `C:\Users\chris\OneDrive\DevGit\JOSYN.POC`; clean-slate migration, no Git history transferred
- **Repo character: PoC v2 / Architecture Showcase** — colleagues as readers/reviewers, not contributors; showcase + discussion ground
- **Dual-view separation** — substance (code, build, docs) and agent layer (persona, instructions, story method) are migrated and documented as two distinct, independently reviewable units
- **Phase 1 complete** — all pre-migration IPC fixes were already in place; no dedicated fix session needed; JOSYN logically sealed by final commit (owner-managed, no formal tag required)
- **Async handler already fixed** — `Func<byte[], byte[]>` → `Func<byte[], Task<byte[]>>` applied throughout; confirmed by code inspection
- **ClientPipes/ServerPipes already fixed** — both are `sealed class` in current code; confirmed by code inspection
- **JOSYN.Core is history** — all current code lives under `JOSYN.Foundation/`; `copilot-instructions.md` in JOSYN corrected accordingly
- **Story Method survives fully** — infrastructure (artifacts, persona, copilot-instructions) migrated; story session content from other stories stays in JOSYN as history
- **copilot-instructions.md rebuilt** — not copied; rebuilt for JOSYN.POC identity in session 0003
- **Session numbering** — migration story sessions in JOSYN.POC continue from session 0004 onward
- **Single-in-flight IPC limitation** — not fixed; documented as known limitation for PoC v2
- **README in JOSYN.POC: showcase quality** — architecture overview, layer diagram, building-block descriptions for colleague-readers — Phase 3 task

## Open Questions

- Phase 3 (showcase README) not yet started

## Sessions

| # | File | Summary |
|---|------|---------|
| 0001 | session-0001-migration-foundation-summary.md | Foundation: migration scope, repo character, phase plan |
| 0002 | session-0002-substance-migration-generation.md | Pre-migration audit (confirmed fixes already in place) + substance migration; 161/161 tests ✅ |
| 0003 | session-0003-agent-layer-migration-generation.md | Agent layer: persona, artifacts, unit-test instructions copied; copilot-instructions.md rebuilt; migration story seeded in JOSYN.POC ✅ |
