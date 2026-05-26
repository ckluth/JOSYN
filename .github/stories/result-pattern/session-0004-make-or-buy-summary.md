# Result Pattern — Make or Buy? (Summary)

**Session date:** 2026-05-21  
**Conclusion:** Keep the custom implementation.

---

## Context

Evaluated whether `JOSYN.Core.ResultPattern` should be replaced by a well-supported NuGet package.  
Packages researched: **FluentResults** (2.6k★), **ErrorOr** (~7k★), **Ardalis.Result** (1k★), **OneOf** (4k★), **LanguageExt** (7k★).

---

## JOSYN's Differentiating Features

Three features are **not present in any of the five packages**:

| Feature | What it does |
|---|---|
| `[CallerMemberName/FilePath/LineNumber]` on `Fail()` | Auto-captures source coordinates at every error origin — zero effort from the caller |
| `Result.Propagate(inner)` | Appends a `CallerInfo` frame each time an error bubbles up — builds a **logical application call chain** |
| `catch (Exception ex) { return ex; }` | Implicit `Exception → Result` conversion via `StackFrame(1)` — cleanest possible catch blocks |

`CallStackAsString` renders the accumulated chain as a readable breadcrumb trail.  
This survives async boundaries and serialization — .NET stack traces do not.

The closest competitor is **FluentResults** with `Error.CausedBy()` — but that is a manually-assembled domain-level *why* tree, not an automatic code-level *where* chain.

---

## Feature Comparison (abbreviated)

| Feature | JOSYN | FluentResults | ErrorOr | Ardalis | LanguageExt |
|---|:---:|:---:|:---:|:---:|:---:|
| Auto CallerInfo capture | ✅ | ❌ | ❌ | ❌ | ❌ |
| `Propagate()` call chain | ✅ | ❌ | ❌ | ❌ | ❌ |
| `implicit (Exception)` | ✅ | ❌ | ❌ | ❌ | ❌ |
| `implicit T → Result<T>` | ✅ | ✅ | ✅ | ✅ | ✅ |
| Multiple errors / result | ❌ | ✅ | ✅ | ✅ | ✅ |
| Railway `.Then()/.Bind()` | ❌ | ✅ | ✅ | ✅ | ✅ |
| `record`-based immutable | ✅ | ❌ | ✅ | ❌ | mixed |

---

## Decision

**Keep the custom implementation.** The `Propagate()` + CallerInfo chain is a debugging superpower for a no-exception architecture. Maintenance burden is low (small, stable library). The only missing commodity features are railway operators (`.Then()`/`.Bind()`), which could be added additively if ever needed.

---

## Side note: Copilot workflow improvements (same session)

- Updated `.github\copilot-instructions.md` with a new discussion file naming convention:  
  `session-NNN-[brief-slug]-[type].md` inside `discussions\[topic]\`  
  Types: `discussion` | `summary` | `conclusion` | `analysis` | `generation`
- Trigger: when user says "save", "summarize", etc. — always propose filename and confirm before writing.
