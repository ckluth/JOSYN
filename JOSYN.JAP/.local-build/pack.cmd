@echo off
cd /d "%~dp0.."
dotnet pack ..\JOSYN.JAP --output "..\Local Packages"
pause
