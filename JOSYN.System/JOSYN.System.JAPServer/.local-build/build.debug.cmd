@echo off
call "%~dp0build.cmd" Debug
rem pause
exit /b %ERRORLEVEL%
