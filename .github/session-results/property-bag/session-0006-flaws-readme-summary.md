# Session 0006 — Flaws, Limitations & README

## Goals

1. Thorough discussion of flaws, limitations, improvement suggestions, and the entitlement of this package to be public.
2. Write the thorough NuGet `README.md` for `JOSYN.Core.PropertyBag`.

---

## Discussion

### 🔴 Flaws (actual problems)

**1. Global culture side-effect** (`PropertyBag` + `JsonDictionarySerializer` static ctors)
Both set `DefaultThreadCurrentCulture` to `de-DE` in their static constructors. A library has no business touching process-wide state. If a host app has configured a different culture, loading this package silently overrides it for every thread. `JsonDictionarySerializer`'s static ctor is also redundant with `PropertyBag`'s — the open question in the `_index.md` answers itself: both should be removed. The correct fix is to document that the host process is responsible for culture setup, and expose a `PropertyBag.Configure(CultureInfo)` method if explicit setup is needed.

**2. Primary-constructor records cannot be deserialized**
`record MyRecord(string Name, int Age)` — the most idiomatic C# syntax — serializes fine but fails on deserialization. `Activator.CreateInstance` throws `MissingMethodException` for records without a parameterless constructor, which positional records don't have. The entire test suite sidesteps this by using `init`-property style exclusively. This limitation is nowhere documented. Fix: detect the canonical constructor, match its parameter names to dictionary keys, and use it instead of `CreateInstance + SetValue`.

**3. `CreateCultureAwareOptions()` creates new `JsonSerializerOptions` per call**
`JsonSerializerOptions` is expensive to construct — it builds internal reflection caches. Creating a fresh instance per serialize/deserialize call is a known `System.Text.Json` antipattern. Should be a static readonly cached instance.

**4. Compiler-internal record detection**
`type.GetMethod("<Clone>$") is not null` relies on a compiler-generated, non-specified internal method name. Works in all current compilers and is unlikely to change, but is technically fragile. Should be documented as a known fragility.

### 🟡 Limitations (by design, but must be documented)

**5. `init`-property record pattern required for deserialization** — see flaw #2 above. Design decision or flaw depending on perspective, but must be prominently stated.

**6. Flat records only** — No nested records, no collections, no arrays.

**7. `DateTimeOffset` is missing from supported types** — Modern code prefers `DateTimeOffset` over `DateTime`. Trivial to add.

**8. INI values are verbatim** — No trimming of the right-hand side. `Key= value` captures the leading space. By design (fixed in session 0002), but easy to get wrong by hand.

**9. Single-culture assumption** — Numbers and dates serialize in the active thread culture (default `de-DE`). A `decimal` of `3.14` serializes as `"3,14"`. Internally consistent, but opaque to consumers from other locales.

**10. INI key matching is case-sensitive during record deserialization** — The first-character case toggle only applies to the `ParameterInfo[]` overload. `Deserialize<TRecord>` uses exact property-name matching. Hand-written INI with lowercase keys will fail for PascalCase properties.

### 🟢 Improvement suggestions

- **Remove both culture static constructors** from `PropertyBag` and `JsonDictionarySerializer`. Document that the host process owns culture setup.
- **Support canonical-constructor deserialization** — remove the biggest usability barrier.
- **Cache `JsonSerializerOptions`** — store as a static readonly field in `JsonDictionarySerializer`.
- **Add `DateTimeOffset`** — one entry in `SupportedPropertyTypes`, one case in `ConvertFromString`.
- **Document `<Clone>$` fragility** in a code comment.

### 📋 Entitlement as a public (local) package

**Yes, with scope constraints.**

PropertyBag solves a real, recurring problem in JOSYN: structured data passing through the IPC named-pipe channel requires a deterministic, inspectable string representation. The API is clean, the delegate-as-plug-in-point pattern (`DictionaryToStringSerializer`) is compositionally elegant, the Result-pattern integration is disciplined, and format auto-detection covers the 90% case neatly.

It should NOT be positioned as a general-purpose serialization library. The culture coupling, the primary-constructor gap, and the flat-only constraint disqualify it from general use. `System.Text.Json` is the right answer for general JSON.

**Verdict:** Entitled for its specific role within JOSYN (IPC message serialization, internal only). Package tags `genesis;poc` are appropriate.

---

## Outcome

- **README.md written** at `JOSYN.Core.PropertyBag\JOSYN.Core.PropertyBag\README.md`
  - Scope disclaimer upfront (not a general-purpose library)
  - Quick-start example
  - API overview table
  - Supported types table
  - Nullable handling
  - Enum serialization
  - INI format details (verbatim values, sectionless, comment lines, duplicate-key error)
  - JSON format details (flat object, culture-aware converters)
  - Culture section with explicit "PropertyBag does NOT set the culture itself" statement
  - Constraints section with prominent ✅/❌ init-property vs. primary-constructor example
  - Delegate types section
  - Parameter deserialization section
  - Dependencies and status

---

## Key decisions reinforced / added

- Culture setup is the host process's responsibility — the library should not set `DefaultThreadCurrentCulture`.
- Primary-constructor records are a known limitation; `init`-property style is the required pattern.
- Entitlement is scoped: internal JOSYN IPC use only.
