# Session 0001 – Story Definition Summary

## Story: poc

## Purpose

This session establishes the shared understanding of the "poc" story goals and structure.
No code was generated; the result is a precise conceptual baseline for future implementation sessions.

---

## High-Level Goals (for the whole story)

- Develop the JOSYN PoC to a maturity level suitable for **show-and-tell with colleagues**.
- Produce a **comprehensive final documentation** so the next steps are clearly defined.
- Reach a **logical archive / frozen state** for the entire JOSYN meta-repo.

### PoC Purpose

- Show-and-tell basis for colleagues.
- Demonstration of the core ideas.
- Documented foundation for further evolution / next milestones.
- Not throw-away code: deliver **mature building-block implementations**.

---

## Target Structure

JOSYN is divided into four logical aspects:

### 1. JOSYN.Foundation

- No dependencies to the other aspects; is the shared base.
- **Not JOSYN-specific** — reusable in other projects.
- Always backwards-compatible; ideally stable at 1.0.0 forever.
- Each building block lives in its own repo with its own versioning and produces its own NuGet package.

#### Building Blocks (exactly three)

| New Name | Current Implementation | Notes |
|---|---|---|
| `JOSYN.Foundation.ResultPattern` | `JOSYN.Core.ResultPattern` | Fully autonomous; referenced everywhere |
| `JOSYN.Foundation.PropertyBag` | `JOSYN.Core.PropertyBag` | Lightweight utility; no contract character |
| `JOSYN.Foundation.JIP` | `JOSYN.Core.IPC` | Heaviest Foundation block; named "JIP" going forward |

**Rename scope:** Full, physical rename — assembly names, namespaces, folder structure.

**JIP specifics:**
- Retains its two demo projects (Client + Server) for documentation purposes only (never public).
- Known PoC limitations (async handler, single-in-flight protocol, etc.) remain documented but are not blocking for the PoC.

**NuGet maturity target for all Foundation packages:**
- Readme, meta-information, complete XML comments (on interfaces/contracts; `inheritdoc` on implementations).

---

### 2. JOSYN.Contract

- Current implementation: `JOSYN.JAP` / `JOSYN.JAP.Protocol`
- Defines **JAP** (JOSYN Application Protocol) as a contract — the high-level JIP protocol for the future JOSYN system.
- For the PoC: demo members only; content can be taken 1:1.
- **Naming is unsatisfactory** (namespace, artifact, position in overall structure) — new naming TBD in a future session.
- Currently "dirty" for a public NuGet package.
- **NuGet maturity target:** same as Foundation (Readme, meta, XML comments).

---

### 3. JOSYN.Frontend

- Current implementation: `JOSYN.JobHost` + `JOSYN.MyDemoJob`
- In the PoC: represents a **JAP-client implementation**.
- Contains:
  - `JOSYN.JobHost` — the central NuGet package for job development (greatest possible decoupling from backend via JIP).
  - A demo job implementation.
- References: `JOSYN.Foundation` + `JOSYN.Contract`

---

### 4. JOSYN.Backend

- Current implementation: `JOSYN.System.JAPServer`
- In the PoC: represents a **JAP-server implementation** (first dummy implementation for JAP).
- Currently largely a copy of the IPC demo server.
- References: `JOSYN.Foundation` + `JOSYN.Contract`

---

## Agreed Implementation Order

Logical layering, bottom-up:

1. `JOSYN.Foundation.ResultPattern`
2. `JOSYN.Foundation.PropertyBag`
3. `JOSYN.Foundation.JIP`
4. `JOSYN.Contract` (naming TBD before starting)
5. `JOSYN.Frontend`
6. `JOSYN.Backend`
7. Final documentation + logical archive

---

## Open Questions

- **JOSYN.Contract naming:** Deliberately left open. Must be explicitly clarified at the right time before implementation of that layer starts — it is a tracked, time-critical decision point, not forgotten.

## Clarified Points

- **Logical archive:** Does NOT require any concrete action in this PoC. It means: the next milestone *could* be continued on a completely new repo without information loss. The PoC must be structured so that this would scale — but no archiving mechanism needs to be built.
