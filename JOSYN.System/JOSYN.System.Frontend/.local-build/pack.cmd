@echo off
cd /d "%~dp0.."
dotnet pack JOSYN.System.Frontend.JobHost --output "..\..\Local Packages"
pause
