# Session 0005 — ECHO Demo Case

**Date:** 2026-05-22  
**Type:** generation

## Goal

ECHO-Funktion als Demo für Request-mit-Payload → Response-mit-Payload ergänzen.

## Changes

**`Demo.ServerExe/Program.cs`**
```csharp
"ECHO" => Result<string?>.Success("ECHO " + req.Data),
```

**`Demo.ClientExe/Program.cs`**
```csharp
// --- ECHO (string-Payload round-trip) ---
var echo = await JipClient.SendAsync(pipes, "ECHO", "Hallo JOSYN");
PrintResult("ECHO", echo.ToResult(), echo.Value);
if (!echo.Succeeded) return 1;
```

## Build Result

`dotnet build --configuration Release` → **Erfolgreich** (0 Fehler, 0 Warnungen)
