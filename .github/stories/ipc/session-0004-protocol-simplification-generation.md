# Session 0004 — JIP Protocol Simplification

**Date:** 2026-05-22  
**Type:** generation

## Goal

Radically simplify the JIP convention layer: remove `Dictionary<string, string>` from both
`Request` and `Response`, unify all handler return types to `Result<string?>`, and make the
application layer solely responsible for payload interpretation and serialization.

---

## Changes Made

### Deleted
- `Jip/Wire/ResponseStatus.cs` — enum with `Success / TechnicalFailure / LogicalFailure` no longer needed

### Modified — Wire Types

**`Request`**
- Removed `Dictionary<string, string>? Dict`

**`Response`**
- Removed `Dictionary<string, string>? Dict`
- Replaced `ResponseStatus Status` + `bool HasError` with `bool Succeeded`
- `[MemberNotNullWhen(false, nameof(Error))]` applied to `Succeeded`

### Modified — Contracts

**`IRequest`** — `Dict` removed

**`IResponse`** — `Dict` removed, `ResponseStatus Status` + `HasError` → `bool Succeeded`

**`IJipProtocol`** — stripped to 4 members:
```csharp
static abstract Result<Request>  ParseRequest(string raw);
static abstract Result<Response> ParseResponse(string raw);
static abstract Response         ToResponse(Result<string?> result);
static abstract Result<string?>  ToResult(Response response);
```
Gone: `ToResponse(Result)`, `ToResponse<T>(Result<T>, Func<T,string>)`, `ToLogicalFailureResponse`, `ToResult(Response)` (void variant), `ToResult<T>(Response, Func<string,T>)`

### Modified — Implementation

**`JipProtocol`** — implements the simplified `IJipProtocol`; no more serialize/deserialize delegates

**`JipClient`** — reduced to a single method:
```csharp
public static Task<Result<string?>> SendAsync(ClientPipes pipes, string what, string? data = null)
```
Gone: `SendAsync<TIn>`, `SendAsync<TOut>`, `SendAsync<TIn,TOut>`, `SendDictAsync`, private `SendCoreAsync`

**`JipServer.WrapHandler`** — handler signature changed from `Func<Request, Response>` /
`Func<Request, Task<Response>>` to `Func<Request, Result<string?>>` /
`Func<Request, Task<Result<string?>>>`

### Modified — Demo Projects

**`Demo.ServerExe/Program.cs`** — dispatch switch now returns `Result<string?>` directly:
```csharp
"PING"       => Result<string?>.Success(null),
"GET-CONFIG" => Result<string?>.Success("{ \"version\": \"1.0\", \"mode\": \"demo\" }"),
_            => Result<string?>.Fail($"Unbekannte Funktion: '{req.What}'"),
```
Removed: `GET-DICT` case (Dict eliminated)

**`Demo.ClientExe/Program.cs`** — uses single `SendAsync`, no deserialize delegate, no `SendDictAsync`

---

## Key Design Decisions

- **`Result<string?>`** as the canonical handler return type — both "void" (null value) and
  "with payload" (non-null string) go through the same path. The app layer null-checks `Value`
  only when it expects data.
- **`bool Succeeded` instead of `ResponseStatus` enum** — the two-failure-type distinction
  (logical / technical) moved entirely to the error message text, which is the app layer's
  responsibility.
- **No serialize/deserialize delegates in JIP layer** — the JIP layer is now purely a
  transport adapter; type mapping belongs to the callers.

## Build Result

`dotnet build --configuration Release` → **Erfolgreich** (3 projects, 0 errors, 0 warnings)
