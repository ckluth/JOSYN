# PropertyBag – Story Index

## Key Decisions

- INI format is whitespace-exact for values (no trimming). Hand-crafted INI with spaces around `=` will capture the leading space in the value — caller's responsibility.
- All public static types have `static abstract` interface contracts in `Contracts/` (mirrors the IPC `Contracts/` pattern). These are documentation contracts, not polymorphism.
- `IniDictionarySerializer` is a `static class` (consistent with the functional-first, static-wins style). The interface `IIniDictionarySerializer` exists as a documentation contract only; the static class references it via `/// <inheritdoc cref="..."/>` exactly like `PropertyBag` and `JsonDictionarySerializer`.
- XML docs live on interfaces; implementations use `/// <inheritdoc/>` (non-static) or `/// <inheritdoc cref="..."/>` (static classes).
- **Culture setup is the host process's responsibility.** The library must NOT set `DefaultThreadCurrentCulture` itself. The static constructors that did this have been removed.
- **`JosynCulture.Default`** (in `JOSYN.Core.PropertyBag`) is the single source of truth for the canonical JOSYN culture (`de-DE`). It is a compile-time constant. Do not make it configurable at runtime — a culture mismatch between writer and reader causes silent data-corruption. Host processes apply it at startup; libraries never touch it.
- **Both record styles are supported for deserialization.** Init-property style uses `Activator.CreateInstance` + `SetValue`. Primary-constructor (positional) style detects the absence of a parameterless constructor, finds the matching public constructor (non-nullable params must be present in dict; nullable params may be absent), and invokes it directly.
- **`DateTimeOffset`** is a supported property type.

---

## Open Questions

_(none — all known issues resolved as of session 0008)_

---

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-first-impression-analysis.md | First full read-through: structure, data flow, key design decisions, and 7 observations/issues identified |
| 0002 | session-0002-flaw-fixes-summary.md | Fixed two real bugs: `Deserialize<TRecord>` swallowing inner failures; INI serializer trimming string values |
| 0003 | session-0003-unit-test-suite-generation.md | Replaced temporary smoke test with a clean 42-test suite across 4 files covering all main behaviour paths |
| 0004 | session-0004-interface-contracts-documentation.md | Added `IPropertyBag`, `IIniDictionarySerializer`, `IJsonDictionarySerializer` contract interfaces; moved all XML docs to interfaces; implementations use `inheritdoc` |
| 0008 | session-0008-preview01-milestone-summary.md | Closed all open items: primary-constructor deserialization, `DateTimeOffset`, cached `JsonSerializerOptions`, `<Clone>$` comment; 47 tests green; README updated; 1.0.0-preview01 ready to pack |
