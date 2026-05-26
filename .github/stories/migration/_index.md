# Story: migration

## Key Decisions

- **New repo is `JOSYN.POC`** — already created; clean-slate migration, no Git history transferred
- **Repo character: PoC v2 / Architecture Showcase** — colleagues as readers/reviewers, not contributors; showcase + discussion ground
- **Fix before migrate** — all known IPC open issues resolved in JOSYN before any code is moved to JOSYN.POC
- **Story Method survives fully** — infrastructure (artifacts, persona, copilot-instructions) migrates; story session content stays in JOSYN as history
- **Single-in-flight IPC limitation** — not fixed; documented as known limitation for PoC v2
- **copilot-instructions.md must be rebuilt** — current version references outdated `JOSYN.Core` structure; will be updated during agent-layer migration
- **JOSYN.POC branch strategy: `main`** — clean linear history; no feature branch workflow required
- **README in JOSYN.POC: showcase quality** — architecture overview, layer diagram, building-block descriptions for colleague-readers

## Open Questions

- Should the async IPC handler cascade through JipClient as well, or only the server side?
- Should JOSYN receive a formal milestone marker (Git tag, branch rename) before sealing?
- Session numbering: migration story sessions in JOSYN.POC continue from last JOSYN session number.

## Sessions

| # | File | Summary |
|---|------|---------|
| 0001 | session-0001-migration-foundation-summary.md | Foundation session: migration scope defined, new repo character established, phase plan outlined |
