@echo off
call "%~dp0zzz.build.cmd" Release
exit /b %ERRORLEVEL%
