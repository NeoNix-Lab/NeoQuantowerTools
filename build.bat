@echo off
setlocal


:: === CONFIG ===
set SOLUTION_NAME=NeoQuantowerTools.sln
set BUILD_CONFIG=Release
set DOTNET=dotnet

:: === BUILD ===
echo Building all projects in solution: %SOLUTION_NAME%
%DOTNET% build %SOLUTION_NAME% -c %BUILD_CONFIG%
pause
if %errorlevel% neq 0 (
    echo Build failed.
    pause
    exit /b %errorlevel%
)
echo Build complete.

:: === PACK ===
echo Packing NuGet for Abstractions...
%DOTNET% pack src\Neo.Quantower.Abstractions\Neo.Quantower.Abstractions.csproj -c %BUILD_CONFIG% --no-build
if %errorlevel% neq 0 (
    echo ❌ NuGet pack failed.
    pause
    exit /b %errorlevel%
)

echo NuGet package created.

echo All done.
pause
