@echo off
setlocal EnableDelayedExpansion

:: ============================================================
:: clean-nuget-package.cmd
:: Aufruf: clean-nuget-package.cmd <paketname>
:: Beispiel: clean-nuget-package.cmd newtonsoft.json
:: ============================================================

set "PACKAGE=%~1"

if "%PACKAGE%"=="" (
    echo [FEHLER] Kein Paketname angegeben.
    echo Aufruf: %~nx0 ^<paketname^>
    exit /b 1
)

:: NuGet-Cache-Pfad dynamisch ermitteln
set "NUGET_CACHE=%USERPROFILE%\.nuget\packages"

if not exist "%NUGET_CACHE%" (
    for /f "delims=" %%i in ('dotnet nuget locals global-packages --list 2^>nul') do (
        set "LINE=%%i"
        set "NUGET_CACHE=!LINE:global-packages: =!"
    )
)

if not exist "%NUGET_CACHE%" (
    echo [FEHLER] NuGet-Cache nicht gefunden: %NUGET_CACHE%
    exit /b 1
)

set "PKG_PATH=%NUGET_CACHE%\%PACKAGE%"

if exist "%PKG_PATH%" (
    echo [LOESCHEN]  %PACKAGE%
    echo             %PKG_PATH%
    rd /s /q "%PKG_PATH%"
    if !errorlevel! == 0 (
        echo             ^> Erfolgreich geloescht.
        exit /b 0
    ) else (
        echo             ^> [FEHLER] Loeschen fehlgeschlagen!
        exit /b 1
    )
) else (
    echo [NICHT DA]  %PACKAGE%  ^(nicht im Cache^)
    exit /b 0
)

endlocal
