@echo off
echo Cleaning previous builds...

cd src\NeoQuantowerToolkit
rd /s /q bin
rd /s /q obj

echo Building NeoQuantowerToolkit in Release mode...
dotnet build -c Release

if %errorlevel% neq 0 (
    echo Build failed.
    exit /b %errorlevel%
)

echo Packing NuGet package...
dotnet pack -c Release

if %errorlevel% neq 0 (
    echo Pack failed.
    exit /b %errorlevel%
)

echo Done!
cd ../..
