@echo off
echo Building Confluence Copier as self-contained executable...
echo.

REM Clean previous builds
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "publish" rmdir /s /q "publish"

REM Build self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

if %errorlevel% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo Executable location: publish\ConfluenceCopier.exe
echo.
echo The executable is fully self-contained and doesn't require .NET runtime to be installed.
echo.
pause 