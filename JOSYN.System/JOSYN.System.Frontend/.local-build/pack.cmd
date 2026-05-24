@echo off
cd /d "%~dp0.."
dotnet pack JOSYN.System.Frontend --output "..\..\Local Packages"
pause
