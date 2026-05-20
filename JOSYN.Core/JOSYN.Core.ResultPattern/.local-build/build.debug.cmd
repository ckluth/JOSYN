@echo off
call "%~dp0build.cmd" Debug
pause
exit /b %ERRORLEVEL%
