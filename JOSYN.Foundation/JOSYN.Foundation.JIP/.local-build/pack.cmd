@echo off
cd /d "%~dp0.."
dotnet pack JOSYN.Foundation.JIP --output "..\..\Local Packages"
pause
