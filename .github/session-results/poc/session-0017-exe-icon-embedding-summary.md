# Session 0017 — Exe Icon Embedding

## Problem

Windows Explorer showed the default .NET icon for all exe projects, even after placing an `icon.ico`
file co-located with the csproj and referencing it via `<None Include="icon.ico"/>`.

## Root Cause

`<None Include="..."/>` only tracks the file in the project; it does not instruct the SDK to embed
it as a Win32 resource. Two things are required together:

1. `<ApplicationIcon>icon.ico</ApplicationIcon>` in `<PropertyGroup>` — tells Roslyn (`csc.exe`) to
   pass `/win32icon:icon.ico` to the compiler, which embeds the icon in the managed DLL.
2. `<Content Include="icon.ico" />` in an `<ItemGroup>` — this is the piece that was missing; without
   it the SDK does not copy/process the ICO file correctly for embedding.

Discovery method: built via Visual Studio UI, compared the resulting csproj delta → VS added
`<Content Include="icon.ico" />`. Confirmed by DLL size increase (+512 bytes) and visual verification
in Windows Explorer.

## Changes Made

- `icon.ico` (provided by user) copied to all four exe projects:
  - `JOSYN.Foundation.JIP.Demo.ClientExe`
  - `JOSYN.Foundation.JIP.Demo.ServerExe`
  - `JOSYN.System.Backend.JAPServer`
  - `JOSYN.System.Frontend.JOSYN.MyDemoJob`
- All four csproj files updated: `ApplicationIcon` added to `PropertyGroup`, `<Content Include="icon.ico" />`
  added, stale `<None Include="icon.png"/>` replaced.
- Template 2 (Exe) in `copilot-instructions/C# Project Files/AGENT.md` updated accordingly.
