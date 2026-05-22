# Session 0007 – Culture Constant

**Date:** 2026-05-21  
**Story:** property-bag

---

## Topic

How to enforce `de-DE` consistently across all JOSYN processes — regardless of machine localization — while keeping the setting explicit, documented, and easy to change in theory, without exposing it as a runtime configuration vulnerability.

---

## The Design Question

The apparent dilemma: *hard-coded* feels hidden; *configurable* feels safe — but for JOSYN the opposite is true. A runtime-configurable culture is a silent data-corruption risk: if a writer and a reader ever use different cultures, numbers and dates round-trip incorrectly with no error raised. The "safe" choice is therefore the compile-time constant.

The real answer is that *hard-coded* and *hidden* are orthogonal:

| | Hard-coded | Configurable |
|---|---|---|
| **Visible** | ✅ `JosynCulture` (chosen) | runtime env-var, app-config |
| **Hidden** | magic string in static ctor (old state) | — |

---

## Decision

Introduce `JosynCulture` — a `public static class` with a single `public static readonly CultureInfo Default` — as the single source of truth for JOSYN's canonical culture.

**Placement: `JOSYN.Core.PropertyBag`**  
`ResultPattern` is culture-agnostic and must stay that way. `PropertyBag` is the only JOSYN building block that needs culture, so it is the natural and correct home. Host processes that apply `JosynCulture.Default` at startup already reference `PropertyBag` — discoverability is direct.

---

## Implementation

### New file: `JOSYN.Core.PropertyBag/JosynCulture.cs`

```csharp
public static class JosynCulture
{
    /// <summary>
    /// The canonical culture used throughout all JOSYN processes.
    /// Currently <c>de-DE</c>. See class remarks before changing this value.
    /// </summary>
    public static readonly CultureInfo Default = CultureInfo.GetCultureInfo("de-DE");
}
```

Full XML doc on the class explains:
- Why culture is fixed (silent data-corruption risk if writer ≠ reader)
- This is a compile-time constant — change it by editing this file and rebuilding
- Do NOT make it configurable at runtime
- Host processes apply it at startup; libraries must not touch `DefaultThreadCurrentCulture`

### Removed

Both `static` constructors in `PropertyBag.cs` and `JsonDictionarySerializer.cs` that previously set `DefaultThreadCurrentCulture` were removed. The unused `using System.Globalization;` in `JsonDictionarySerializer.cs` was cleaned up.

### README updated

The **Culture** section in `README.md` now references `JosynCulture.Default` as the single source of truth and shows the startup snippet.

---

## Verification

- `ResultPattern` builds clean (0 errors) — confirms no culture concern leaked there.
- `PropertyBag` builds clean (0 errors).
- All 42 PropertyBag tests pass.
