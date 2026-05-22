# PropertyBag – Session 0004: Interface Contracts & XML Documentation

## Goal

Ensure the entire codebase is described via interfaces. Rather than for polymorphism, these interfaces serve as explicit API contracts that define the shape of every public component. Static classes get interfaces with `static abstract` members. XML documentation is consolidated on the interfaces; implementations refer to it via `/// <inheritdoc/>` (or `/// <inheritdoc cref="..."/>` for static classes).

---

## What Was Done

### New: `Contracts/` folder

Three interface files were created under `JOSYN.Core.PropertyBag/Contracts/`, mirroring the established IPC pattern:

| File | Interface | Purpose |
|------|-----------|---------|
| `IPropertyBag.cs` | `IPropertyBag` | Contract for the main serialization/deserialization entry point |
| `IIniDictionarySerializer.cs` | `IIniDictionarySerializer` | Contract for INI serialize/deserialize operations |
| `IJsonDictionarySerializer.cs` | `IJsonDictionarySerializer` | Contract for JSON serialize/deserialize operations |

All interfaces use C# 11 `static abstract` members, as every method on the implementing types is static.

### Implementation changes

| File | Change |
|------|--------|
| `PropertyBag.cs` | Class and all public methods reference interface docs via `/// <inheritdoc cref="IPropertyBag.xxx"/>` |
| `IniDictionarySerializer.cs` | Class now formally implements `IIniDictionarySerializer`; uses plain `/// <inheritdoc/>` |
| `JsonDictionarySerializer.cs` | Class and all public methods reference interface docs via `/// <inheritdoc cref="IJsonDictionarySerializer.xxx"/>` |
| `DelegateTypes/DictionaryToStringSerializer.cs` | Full XML docs written directly (delegates have no interface counterpart) |
| `DelegateTypes/StringToDictionarySerializer.cs` | Full XML docs written directly |
| `CultureAwareConverters/*.cs` | Class-level summary added; `Read`/`Write` overrides use `/// <inheritdoc/>` |
| `SupportedPropertyTypes.cs` | Class-level summary + `IsMatch` method doc added (internal) |

### Key design decisions

- **`IniDictionarySerializer` is non-static** and therefore can formally implement `IIniDictionarySerializer` (like `PipesServer : IPipesServer` in IPC). Plain `/// <inheritdoc/>` works without `cref`.
- **`PropertyBag` and `JsonDictionarySerializer` are static classes** — they cannot implement interfaces in C#. For these, `/// <inheritdoc cref="IXxx.Method(...)"/>` provides the explicit link and IntelliSense will pull docs from the interface.
- The open question "Should `IniDictionarySerializer` be made `static`?" remains open. Making it static would require removing the `IIniDictionarySerializer` implementation (`static class` cannot implement an interface in C#). For now, keeping it non-static is the right call as it allows the cleaner `/// <inheritdoc/>` pattern.

---

## Verification

All 42 existing tests pass (`dotnet test` green).
