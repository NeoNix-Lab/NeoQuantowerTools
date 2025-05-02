@echo off
echo.
echo *** Building and packing Neo.Quantower.Toolkit (net8) ***
echo.

:: 1. Build solution or project
dotnet build -c Release

:: 2. Check if DLL exists
if exist bin\Release\net8.0\Neo.Quantower.Toolkit.dll (
    echo.
    echo Packing NuGet package...

    :: 3. Create the .nupkg using dotnet pack instead of nuget pack
    dotnet pack -c Release

    echo.
    echo *** Package .nupkg successfully created! ***
) else (
    echo.
    echo ERROR: net8.0 DLL not found! Build failed or wrong path.
)

pause
