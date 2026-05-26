# Session 0004 — JOSYN.Foundation.PropertyBag: Physical Rename

**Story:** poc  
**Branch:** poc/evolution  
**Type:** generation

---

## What Was Done

Physical rename of `JOSYN.Core.PropertyBag` → `JOSYN.Foundation.PropertyBag`.  
Same pattern as session 0003 (ResultPattern), executed cleanly end-to-end.

---

## New Structure

```
JOSYN.Foundation/
  JOSYN.Foundation.PropertyBag/
    JOSYN.Foundation.PropertyBag.slnx
    nuget.config                          → ..\..\Local Packages\
    Directory.Build.props
    README.md                             (repo-level)
    .local-build/
      build.cmd / build.debug.cmd / build.release.cmd / test.cmd / pack.cmd
    JOSYN.Foundation.PropertyBag/
      JOSYN.Foundation.PropertyBag.csproj
        PackageId:    JOSYN.Foundation.PropertyBag
        AssemblyName: JOSYN.Foundation.PropertyBag
        RootNamespace: JOSYN.Foundation.PropertyBag
        PackageReference: JOSYN.Foundation.ResultPattern 1.0.0-preview01
      README.md                           (NuGet-level, inside project folder)
      icon.png
      Contracts/
        IPropertyBag.cs
        IIniDictionarySerializer.cs
        IJsonDictionarySerializer.cs
      DelegateTypes/
        DictionaryToStringSerializer.cs
        StringToDictionarySerializer.cs
      DictionarySerializers/
        IniDictionarySerializer.cs
        JsonDictionarySerializer.cs
      CultureAwareConverters/
        CultureAwareDateOnlyConverter.cs
        CultureAwareDateTimeConverter.cs
        CultureAwareDecimalConverter.cs
        CultureAwareTimeOnlyConverter.cs
      JosynCulture.cs
      SupportedPropertyTypes.cs
      PropertyBag.cs
    JOSYN.Foundation.PropertyBag.Test/
      JOSYN.Foundation.PropertyBag.Test.csproj
      PropertyBagTests.cs
      IniDictionarySerializerTests.cs
      JsonDictionarySerializerTests.cs
      PropertyBagParameterDeserializeTests.cs
```

---

## Changes vs. Core Version

| Aspect | Old | New |
|---|---|---|
| Namespace | `JOSYN.Core.PropertyBag` | `JOSYN.Foundation.PropertyBag` |
| Test namespace | `JOSYN.Core.PropertyBag.Test` | `JOSYN.Foundation.PropertyBag.Test` |
| Assembly/Package ID | `JOSYN.Core.PropertyBag` | `JOSYN.Foundation.PropertyBag` |
| Dependency | `JOSYN.Core.ResultPattern` | `JOSYN.Foundation.ResultPattern` |
| Solution file | `JOSYN.CorePropertyBag.slnx` (quirky) | `JOSYN.Foundation.PropertyBag.slnx` (correct) |

All `using` statements updated; `#pragma warning disable/restore IDE0130` pattern applied throughout.

---

## Outcome

- **Tests:** 47/47 green  
- **Pack:** `Local Packages\JOSYN.Foundation.PropertyBag.1.0.0-preview01.nupkg` ✅  
- **Old directory removed:** `JOSYN.Core/JOSYN.Core.PropertyBag/` ✅  
- **Committed & pushed:** branch `poc/evolution`
