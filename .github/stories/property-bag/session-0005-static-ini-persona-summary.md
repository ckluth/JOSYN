# PropertyBag – Session 0005: IniDictionarySerializer Static + Collaboration Persona

## Context

Immediate follow-up to session-0004. Two topics were processed in the same exchange.

---

## Topic 1 — `IniDictionarySerializer` made `static`

Session-0004 had concluded that `IniDictionarySerializer` must stay non-static so it could formally implement `IIniDictionarySerializer`. That reasoning was wrong by priority: the interfaces in this codebase are documentation contracts, not polymorphism contracts. The static class never had to formally implement anything.

**Change:** `public class IniDictionarySerializer : IIniDictionarySerializer` → `public static class IniDictionarySerializer`. All four methods now use `/// <inheritdoc cref="IIniDictionarySerializer.xxx"/>`, consistent with `PropertyBag` and `JsonDictionarySerializer`.

**Principle confirmed:** *Static wins.* When in doubt, static is the default. OOP/instance patterns are tools pulled in only when they earn their place.

All 42 tests pass after the change.

---

## Topic 2 — Collaboration persona file

Created `.github/copilot-persona.md` to capture the project owner's functional-first C# style as a persistent working agreement. The file includes an **agent-confirm protocol**: at the start of each session the agent paraphrases the key principles before starting work.

### Principles recorded

1. **Static wins** — `static class` is the default; instance types must earn their place
2. **Immutability by default** — `record`, `readonly`, `init`
3. **Pure functions** — side effects isolated and pushed to the edges
4. **Errors as values** — `Result`/`Result<T>`, no `throw` above the lowest catch boundary
5. **Interfaces as contracts** — documentation shape, not polymorphism
6. **Explicit over magic** — no DI containers, no hidden reflection wiring
7. **Minimal surface area** — every public member earns its place

The guiding mindset: *C# written as if it could be F#* — not a functional language zealot, but always asking "would this translate naturally into a functional style?"
