# PropertyBag – Story Index

## Key Decisions

- INI format is whitespace-exact for values (no trimming). Hand-crafted INI with spaces around `=` will capture the leading space in the value — caller's responsibility.
- All public static types have `static abstract` interface contracts in `Contracts/` (mirrors the IPC `Contracts/` pattern). These are documentation contracts, not polymorphism.
- `IniDictionarySerializer` is a `static class` (consistent with the functional-first, static-wins style). The interface `IIniDictionarySerializer` exists as a documentation contract only; the static class references it via `/// <inheritdoc cref="..."/>` exactly like `PropertyBag` and `JsonDictionarySerializer`.
- XML docs live on interfaces; implementations use `/// <inheritdoc/>` (non-static) or `/// <inheritdoc cref="..."/>` (static classes).

---

## Open Questions

- Should culture setup be centralized (remove redundant static ctor in `JsonDictionarySerializer`)?

---

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-first-impression-analysis.md | First full read-through: structure, data flow, key design decisions, and 7 observations/issues identified |
| 0002 | session-0002-flaw-fixes-summary.md | Fixed two real bugs: `Deserialize<TRecord>` swallowing inner failures; INI serializer trimming string values |
| 0003 | session-0003-unit-test-suite-generation.md | Replaced temporary smoke test with a clean 42-test suite across 4 files covering all main behaviour paths |
| 0004 | session-0004-interface-contracts-documentation.md | Added `IPropertyBag`, `IIniDictionarySerializer`, `IJsonDictionarySerializer` contract interfaces; moved all XML docs to interfaces; implementations use `inheritdoc` |
| 0005 | session-0005-static-ini-persona-summary.md | Made `IniDictionarySerializer` static (static-wins principle); created `.github/copilot-persona.md` collaboration working agreement |
