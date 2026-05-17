@echo off
setlocal enabledelayedexpansion

:: -------------------------------------------------------
:: Baut alle Solutions (.slnx) in Unterordnern der 1. Ebene
:: -------------------------------------------------------

:: -------------------------------------------------------
:: Aufruf:  build.cmd [Release|Debug]
:: Default: Release
:: -------------------------------------------------------
set "CONFIGURATION=%~1"
if not defined CONFIGURATION set "CONFIGURATION=Release"

:: Nur bekannte Werte erlauben
if /i "%CONFIGURATION%" neq "Release" if /i "%CONFIGURATION%" neq "Debug" (
    echo [FEHLER] Unbekannte Konfiguration: "%CONFIGURATION%"
    echo          Erlaubt: Release, Debug
    exit /b 1
)


set "ROOT=%~dp0"
set "FAILED_COUNT=0"
set "SUCCESS_COUNT=0"
set "SKIP_COUNT=0"
set "FAILED_LIST="

echo ================================================
echo  build-all.cmd
echo  Repo-Root: %ROOT%
echo ================================================
echo.

REM for /d %%D in ("%ROOT%*") do (
for /r "%ROOT%" /d %%D in (*) do (
    set "SLNX_FILE="

    for %%F in ("%%D\*.slnx") do (
        set "SLNX_FILE=%%F"
    )

    if not defined SLNX_FILE (
        echo [SKIP]  %%~nxD  ^(keine .slnx gefunden^)
        set /a SKIP_COUNT+=1
    ) else (
        echo ------------------------------------------------
        echo [BUILD] %%~nxD
        echo         !SLNX_FILE!
        echo.

        dotnet build "!SLNX_FILE!" --configuration %CONFIGURATION%

        if !ERRORLEVEL! neq 0 (
            echo.
            echo [FEHLER] %%~nxD  ^(Exit-Code: !ERRORLEVEL!^)
            set /a FAILED_COUNT+=1
            set "FAILED_LIST=!FAILED_LIST! %%~nxD"
        ) else (
            echo.
            echo [OK]    %%~nxD
            set /a SUCCESS_COUNT+=1
        )
        echo.
    )
)

echo ================================================
echo  Ergebnis:
echo    OK:       %SUCCESS_COUNT%
echo    Fehler:   %FAILED_COUNT%
echo    Skipped:  %SKIP_COUNT%
if defined FAILED_LIST (
    echo.
    echo  Fehlgeschlagen:
    for %%X in (%FAILED_LIST%) do echo    - %%X
)
echo ================================================

pause 

if %FAILED_COUNT% neq 0 exit /b 1
exit /b 0

