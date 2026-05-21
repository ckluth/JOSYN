# PropertyBag – Story Index

## Key Decisions

- INI format is whitespace-exact for values (no trimming). Hand-crafted INI with spaces around `=` will capture the leading space in the value — caller's responsibility.

---

## Open Questions

- Should `IniDictionarySerializer` be made `static`?
- Should culture setup be centralized (remove redundant static ctor in `JsonDictionarySerializer`)?
- What comes next on the path to clean final PoC?

---

## Sessions

| # | File | Summary |
|---|---|---|
| 0001 | session-0001-first-impression-analysis.md | First full read-through: structure, data flow, key design decisions, and 7 observations/issues identified |
| 0002 | session-0002-flaw-fixes-summary.md | Fixed two real bugs: `Deserialize<TRecord>` swallowing inner failures; INI serializer trimming string values |
| 0003 | session-0003-unit-test-suite-generation.md | Replaced temporary smoke test with a clean 42-test suite across 4 files covering all main behaviour paths |
