@echo off
cd /d "%~dp0.."
dotnet pack --output "..\..\Local Packages"
pause
