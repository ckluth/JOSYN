@echo off
CHCP 1252
cd /d "%~dp0.."
dotnet pack JOSYN.System.Contract --output "..\..\Local Packages"
pause
