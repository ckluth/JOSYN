@echo off
:: ============================================================
:: clean-my-packages.cmd
:: Ruft clean-nuget-package.cmd fuer jedes Paket einzeln auf
:: ============================================================

echo.
echo ============================================================
echo  NuGet Batch-Cleaning
echo ============================================================
echo.

call "%~dp0zzz.clean-nuget-package.cmd" josyn.core.resultpattern
call "%~dp0zzz.clean-nuget-package.cmd" josyn.core.propertybag
call "%~dp0zzz.clean-nuget-package.cmd" josyn.core.ipc

echo.
echo ============================================================
echo  Fertig.
echo ============================================================
echo.
pause
