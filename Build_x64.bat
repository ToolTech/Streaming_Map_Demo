@echo off

REM Set the root directory to start the search. Change the path as needed.
set ROOT_DIR=com.saab.map-streamer
for /r "%ROOT_DIR%" %%i in (*.dll) do (
        echo Deleting: %%i
        del "%%i"
)

REM Define variables
set SOLUTION_PATH=%~dp0vs17\Install_Gizmo\Install_Gizmo.sln
echo Solution file path: %SOLUTION_PATH%
set PLATFORM=x64

REM Define MSBuild path variable
SET "MSBUILD_PATH="

REM Define the path to vswhere.exe, adjust this to your installation of Visual Studio
SET "VSWHERE_PATH=C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"

REM Use FOR /F to capture the output of vswhere.exe directly into the MSBUILD_PATH variable
FOR /F "usebackq tokens=*" %%F IN (`"%VSWHERE_PATH%" -latest -requires Microsoft.Component.MSBuild -property installationPath`) DO (
    SET "MSBUILD_PATH=%%F\MSBuild\Current\Bin\amd64\msbuild.exe"
)

REM Check if MSBuild path was found and if so, print it
IF DEFINED MSBUILD_PATH (
    ECHO MSBUILD file path: %MSBUILD_PATH%
) ELSE (
    ECHO MSBuild was not found. Please make sure you have Visual Studio installed.
    EXIT /B 1
)

REM Restore NuGets
echo Restoring NuGet packages for the solution: %SOLUTION_PATH%
"%MSBUILD_PATH%" "%SOLUTION_PATH%" /t:Restore /p:RestorePackagesConfig=true

REM Check if the NuGet restore was successful
IF %ERRORLEVEL% EQU 0 (
    ECHO NuGet packages were successfully restored.
) ELSE (
    ECHO Error occurred during NuGet package restoration.
    EXIT /B 1
)

REM Build the solution for Release configuration
"%MSBUILD_PATH%" "%SOLUTION_PATH%" /t:Clean;Build /p:Configuration=Release /p:Platform=%PLATFORM%

if errorlevel 1 (
    echo Build failed for Release with exit code %errorlevel%
    exit /b %errorlevel%
)
echo Build succeeded for Release