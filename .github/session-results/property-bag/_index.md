# PropertyBag – Story Index

## Key Decisions

- INI format is whitespace-exact for values (no trimming). Hand-crafted INI with spaces around `=` will capture the leading space in the value — caller's responsibility.
- All public static types have `static abstract` interface contracts in `Contracts/` (mirrors the IPC `Contracts/` pattern). These are documentation contracts, not polymorphism.
- `IniDictionarySerializer` is a `static class` (consistent with the functional-first, static-wins style). The interface `IIniDictionarySerializer` exists as a documentation contract only; the static class references it via `/// <inheritdoc cref="..."/>` exactly like `PropertyBag` and `JsonDictionarySerializer`.
- XML docs live on interfaces; implementations use `/// <inheritdoc/>` (non-static) or `/// <inheritdoc cref="..."/>` (static classes).
- **Culture setup is the host process's responsibility.** The library must NOT set `DefaultThreadCurrentCulture` itself. Both static constructors (in `PropertyBag` and `JsonDictionarySerializer`) should be removed in a future cleanup session.
- **Deserialization requires the init-property record pattern.** Primary-constructor (positional) records cannot be deserialized due to the `Activator.CreateInstance` approach. Documented in README. Fix is known: detect the canonical constructor and use it.
- **Entitlement is scoped to internal JOSYN IPC use.** Not a general-purpose serialization library.

---

## Open Questions

- Primary-constructor record deserialization not yet implemented (known gap, documented in README and session 0006).
- `DateTimeOffset` not yet in supported types (trivial to add).
- `CreateCultureAwareOptions()` creates new `JsonSerializerOptions` per call — should be cached.

---

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-first-impression-analysis.md | First full read-through: structure, data flow, key design decisions, and 7 observations/issues identified |
| 0002 | session-0002-flaw-fixes-summary.md | Fixed two real bugs: `Deserialize<TRecord>` swallowing inner failures; INI serializer trimming string values |
| 0003 | session-0003-unit-test-suite-generation.md | Replaced temporary smoke test with a clean 42-test suite across 4 files covering all main behaviour paths |
| 0004 | session-0004-interface-contracts-documentation.md | Added `IPropertyBag`, `IIniDictionarySerializer`, `IJsonDictionarySerializer` contract interfaces; moved all XML docs to interfaces; implementations use `inheritdoc` |
| 0006 | session-0006-flaws-readme-summary.md | Thorough analysis of flaws/limitations/entitlement + wrote the NuGet README.md |
