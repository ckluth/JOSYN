@echo off
call "%~dp0zzz.build.all.cmd" Release
exit /b %ERRORLEVEL%

