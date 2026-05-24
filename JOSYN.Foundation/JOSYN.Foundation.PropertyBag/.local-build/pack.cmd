@echo off
cd /d "%~dp0.."
dotnet pack --output "..\..\Local Packages"
REM pause
