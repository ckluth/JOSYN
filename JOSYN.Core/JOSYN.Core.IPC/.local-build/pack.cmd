@echo off
cd /d "%~dp0.."
dotnet pack JOSYN.Core.IPC --output "..\..\Local Packages"
pause
