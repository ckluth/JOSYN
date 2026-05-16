@echo off
call "%~dp0zzz.build.cmd" Debug
pause
exit /b %ERRORLEVEL%
