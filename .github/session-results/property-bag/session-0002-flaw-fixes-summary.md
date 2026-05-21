# Session 0002 – Flaw Fixes

**Date:** 2026-05-21  
**Story:** property-bag  
**Type:** summary

---

## Context

Following the first-impression analysis (session-0001), two real correctness flaws were identified and fixed. All existing tests remain green.

---

## Fix 1 — `Deserialize<TRecord>` swallowed `DeserializeFromDictionary` failures

**File:** `PropertyBag.cs`

**Problem:**  
The generic `Deserialize<TRecord>` overload only checked whether the dictionary-parse step succeeded. If `DeserializeFromDictionary` then failed (missing required property, unsupported type, reflection error, etc.), its `.Value` was `null`, `as TRecord` returned `null`, the `!` operator suppressed the compiler warning, and the method returned `Result<TRecord>.Success(null)`. The caller saw `Succeeded == true` and crashed on `result.Value`.

The non-generic sibling (`Deserialize(string, Type, ...)`) did not have this bug — it correctly passed the `DeserializeFromDictionary` result straight through. The asymmetry between the two overloads was the key clue.

**Fix:**  
Replaced the one-liner ternary with two explicit checks — one for the dictionary step, one for the record-construction step — each returning a proper `Result.Error` on failure.

```csharp
// Before
return getDict.Succeeded
    ? (DeserializeFromDictionary(getDict.Value, typeof(TRecord)).Value as TRecord)! // Dieser Cast ist safe, wenn Succeeded!
    : Result.Error(getDict.ErrorMessage, getDict.Exception);

// After
if (!getDict.Succeeded) return Result.Error(getDict.ErrorMessage, getDict.Exception);

var getRecord = DeserializeFromDictionary(getDict.Value, typeof(TRecord));
if (!getRecord.Succeeded) return Result.Error(getRecord.ErrorMessage, getRecord.Exception);

return (getRecord.Value as TRecord)!;
```

The cast on the final return is genuinely safe: `getRecord.Succeeded` guarantees the instance is of type `TRecord`.

---

## Fix 2 — INI serializer trimmed string values, causing data loss

**Files:** `IniDictionarySerializer.cs`, `PropertyBag.cs`

**Problem:**  
String property values with leading or trailing whitespace were silently corrupted on round-trip via INI. `" hello "` serialized to `hello` and could never be recovered. JSON serialization was unaffected, making the two formats inconsistent for the same record.

The trimming happened in three places:
1. `IniDictionarySerializer.Serialize` — `kvp.Value.Trim()` in both the sectionless and sectioned write paths
2. `IniDictionarySerializer.Deserialize` — `keyValue[1].Trim()` in the line parser
3. `PropertyBag.DeserializeFromDictionary` — `rawValue = rawValue.Trim()` before type conversion

**Fix:**  
Removed `.Trim()` from values in all three locations. Keys are still trimmed (property names never have meaningful whitespace). The INI format is now whitespace-exact for values — what goes in, comes out.

The consequence for hand-crafted INI: `Name = hello` (spaces around `=`) will now yield value ` hello` (with leading space). The key is still trimmed so `Name ` is read as `Name`. If spaces around `=` must be tolerated, the caller is responsible for not writing them — or the format documentation should note this.

---

## State After Session

- 2 real correctness bugs fixed
- 1 test, still green
- No other substance issues found in this session
