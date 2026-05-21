# Session 0001 – First Impression Analysis

**Date:** 2026-05-21  
**Story:** property-bag  
**Type:** analysis

---

## Project at a Glance

`JOSYN.Core.PropertyBag` serializes/deserializes flat C# `record` types (not classes) to/from `Dictionary<string, string>` and then further to **sectionless INI** or **JSON** strings. A third `Deserialize` overload targets `ParameterInfo[]` for method-invocation scenarios (IPC dispatch).

Baseline state: **1 test, all green.**

---

## Structure

```
JOSYN.Core.PropertyBag/
├── PropertyBag.cs                  ← static entry point
├── SupportedPropertyTypes.cs       ← type whitelist
├── DelegateTypes/
│   ├── DictionaryToStringSerializer.cs   ← delegate: Dictionary<string,string> → Result<string>
│   └── StringToDictionarySerializer.cs   ← delegate: string → Result<Dictionary<string,string>>
├── DictionarySerializers/
│   ├── IniDictionarySerializer.cs        ← sectionless INI (key=value)
│   └── JsonDictionarySerializer.cs       ← JSON, culture-aware
└── CultureAwareConverters/
    ├── CultureAwareDateTimeConverter.cs
    ├── CultureAwareDateOnlyConverter.cs
    ├── CultureAwareTimeOnlyConverter.cs
    └── CultureAwareDecimalConverter.cs
```

---

## Data Flow

**Serialize:**  
`TRecord` → reflection → `Dictionary<string,string>` → `DictionaryToStringSerializer` delegate → `string`

**Deserialize:**  
`string` → format detection (`{` → JSON, else INI) → `StringToDictionarySerializer` → `Dictionary<string,string>` → reflection + `Activator.CreateInstance` + `SetValue` → `TRecord`

---

## Key Design Decisions

| Decision | Detail |
|---|---|
| Record detection | `<Clone>$` method presence (compiler-generated for `record`) |
| Culture | `de-DE` set as `DefaultThreadCurrentCulture` in `PropertyBag`'s static ctor |
| Format auto-detection | Input string trimmed, check `StartsWith('{')` → JSON, else INI |
| Nullable handling | `NullabilityInfoContext` for reference types; `Nullable.GetUnderlyingType` for value types |
| Error strategy | Full Result pattern — no exceptions propagated, `catch (Exception ex) { return ex; }` throughout |

---

## Observations & Potential Issues

### 1. Unsafe cast in generic `Deserialize<TRecord>` (latent bug)
```csharp
return (DeserializeFromDictionary(getDict.Value, typeof(TRecord)).Value as TRecord)!
```
If `DeserializeFromDictionary` succeeds but the returned object is somehow not a `TRecord`, `as` returns `null` and the `!` operator causes a `NullReferenceException` instead of a clean `Result.Fail`. The type is controlled internally so the risk is low in practice, but it bypasses the Result pattern contract.  
**Fix:** check `Value is TRecord typed ? typed : Result.Fail(...)`.

### 2. `IniDictionarySerializer` is a `class`, not `static`
All other serializers (`JsonDictionarySerializer`, `PropertyBag`) are `static`. Minor inconsistency — could be made `static`.

### 3. Culture set in two places
Both `PropertyBag` and `JsonDictionarySerializer` have static constructors that set `de-DE`. The one in `JsonDictionarySerializer` is redundant since `PropertyBag` always runs first, but the coupling is fragile (someone could use `JsonDictionarySerializer` directly without going through `PropertyBag`).

### 4. `ConvertFromString`: `DateOnly`/`TimeOnly`/`TimeSpan` parse without explicit culture
```csharp
return DateOnly.Parse(rawValue);   // uses CurrentCulture implicitly
return TimeOnly.Parse(rawValue);
return TimeSpan.Parse(rawValue);
```
This works because `CurrentCulture` is set to `de-DE`, but it's implicit. Passing `CultureInfo.CurrentCulture` explicitly would be more self-documenting.

### 5. `DeserializeFromDictionary` requires init-settable properties
`Activator.CreateInstance` bypasses `required` compile-time checks. This is intentional and necessary for deserialization, but means runtime behavior silently ignores `required` constraints — missing keys produce a `Result.Error` instead (which is correct), but it's a non-obvious coupling.

### 6. Minimal test coverage
Only one happy-path test. No tests for:
- Nullable properties
- Enum properties
- Unsupported type errors
- Duplicate key errors (INI)
- Missing required property
- `ParameterInfo[]` overload
- Type mismatch / parse errors

### 7. `ParameterInfo[]` overload — interesting use case
This overload (`Deserialize(string raw, ParameterInfo[] parameters)`) returns `object[]` suitable for `MethodInfo.Invoke`. This is clearly designed for IPC method dispatch. It has its own case-insensitive-first-char matching logic.

---

## Summary

The library is small, well-factored, and idiomatically uses the Result pattern. The core design (delegate-based serializer injection, record enforcement via `<Clone>$`, type whitelist) is clean. Main areas to watch going forward:
- **Correctness:** the unsafe cast (#1)  
- **Coverage:** tests are thin  
- **Consistency:** `IniDictionarySerializer` class vs. static, culture setup duplication
