@echo off
chcp 1252
setlocal

set NUGET_BASE=C:\Users\chris\.nuget\packages

:: ============================================================
:: Zu löschende Pakete hier eintragen (Verzeichnisnamen exakt)
:: ============================================================
set PACKAGES=^
  josyn.core.ipc ^
  josyn.core.propertybag ^
  josyn.core.resultpattern

:: ============================================================

for %%P in (%PACKAGES%) do (
    set "DIR=%NUGET_BASE%\%%P"
    if exist "%NUGET_BASE%\%%P" (
        echo Lösche: %NUGET_BASE%\%%P
        rd /s /q "%NUGET_BASE%\%%P"
        if errorlevel 1 (
            echo   FEHLER beim Löschen von %%P
        ) else (
            echo   OK
        )
    ) else (
        echo Nicht gefunden, übersprungen: %NUGET_BASE%\%%P
    )
)

echo.
echo Fertig.
endlocal
pause
