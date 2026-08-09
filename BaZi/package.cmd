@echo off

rem Please change the APP_VERSION manually
set APP_VERSION=1.15

set ROOT_NAME=BaZi_v%APP_VERSION%
set OUTPUT_ZIP=%ROOT_NAME%_Windows_x64.7z
set PUBLISH_DIR=%ROOT_NAME%

echo [1/3] Publishing project (net10.0-windows)...
rem If PublishTrimmed=true will cause assembly reference miss issue, fast resolve this problem is not use it :D
dotnet publish -p:TargetFrameworks=net10.0-windows10.0.19041.0 -f net10.0-windows10.0.19041.0 -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -p:WindowsPackageType=None -o ./%PUBLISH_DIR%

if %ERRORLEVEL% NEQ 0 (
    echo Publish failed!
    pause
    exit /b %ERRORLEVEL%
)

cls
echo.
echo [2/3] Cleaning up unnecessary language folders...
pushd .\%PUBLISH_DIR%
for /d %%i in (*) do (
    set "keep="
    if /i "%%i"=="en-US" set keep=1
    if /i "%%i"=="zh-TW" set keep=1
    if /i "%%i"=="zh-CN" set keep=1
    if /i "%%i"=="zh-Hant" set keep=1
    if /i "%%i"=="zh-Hans" set keep=1
    if /i "%%i"=="wwwroot" set keep=1
    if /i "%%i"=="Includes" set keep=1
    if /i "%%i"=="Configurations" set keep=1
    if /i "%%i"=="Microsoft.UI.Xaml" set keep=1
    if /i "%%i"=="NpuDetect" set keep=1
    
    if not defined keep (
        rd /s /q "%%i"
    )
)
popd

cls
echo.
echo [3/3] Compressing with 7-Zip (Ultra settings)...
if exist %OUTPUT_ZIP% del %OUTPUT_ZIP%

rem Use -xr!*.pdb to recursively exclude PDB files. 
rem Removed enabledelayedexpansion to prevent '!' from being misinterpreted.
7z a -t7z -mx=9 -ms=on -xr!*.pdb %OUTPUT_ZIP% ./%PUBLISH_DIR%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Cleaning up temporary publish folder...
    rd /s /q .\%PUBLISH_DIR%
)

cls
echo.
echo ========================================
echo Done! Archive created: %OUTPUT_ZIP%
echo ========================================
echo.
echo Press any key to exit...
pause > nul
