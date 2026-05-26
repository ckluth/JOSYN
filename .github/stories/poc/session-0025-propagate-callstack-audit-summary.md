# Session 0025 — Propagate Call-Stack Audit

## What happened

Full codebase audit for violations of the `Result.Propagate` convention. Found and corrected
10 locations across 5 production files where failures were either re-wrapped via
`Result.Error(inner.ErrorMessage, inner.Exception)` or passed through with bare `.ToResult<T>()`
— both of which silently discard the accumulated call chain.

## Antipattern 1 — `.ToResult<T>()` without `Propagate`

Converts the result type but adds no call-chain entry; the caller is invisible in the stack.

| File | Lines |
|------|-------|
| `JipClient.cs` | 15, 17 |
| `JAPClient.cs` | 33 |

**Fix:** `return Result<T>.Propagate(inner.ToResult<T>());`

## Antipattern 2 — `Result.Error(inner.ErrorMessage, inner.Exception)` re-wrap

Preserves message + exception but **drops the entire propagation chain** — the worst variant.

| File | Lines |
|------|-------|
| `PropertyBag.cs` | 182, 234–235, 273, 279, 282 |
| `IniDictionarySerializer.cs` | 49–50 |
| `JobInvoker.cs` | 178–179, 184–185 |

**Fix:** `return Result<T>.Propagate(inner[.ToResult<T>()]);`

## Verification

All 238 tests pass across all affected solutions after the changes:

- `JOSYN.Foundation.ResultPattern` — 125 passed
- `JOSYN.Foundation.PropertyBag` — 61 passed
- `JOSYN.Foundation.JIP` — 49 passed (48 + 1 demo-server test)
- `JOSYN.System.Frontend` — 3 passed
