@echo off

REM Set the root directory to start the search. Change the path as needed.
set ROOT_DIR=com.saab.map-streamer
for /r "%ROOT_DIR%" %%i in (*.dll) do (
        echo Deleting: %%i
        del "%%i"
)

echo Cleaned for BTA