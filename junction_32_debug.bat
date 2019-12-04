rem ---- DONT RUN THIS as a GIT USER, No need --------------------------------
rem ---- Set up all paths in the project as relative paths - Saab Dev Only ---

rem start /WAIT junction_libs.bat 

if not defined GIZMOSDK (
  set GIZMOSDK=..\GizmoSDK
)

rem ------------- runtime dlls -----------------------------------------------

rmdir /S / Q Assets\Plugins
mkdir Assets\Plugins\Android\Libs

ren ............. plugin selections -----------------------------------------

mklink /J Assets\Plugins\x86				%GIZMOSDK%\build\ws\vs15\GizmoSDK_Unity_Libs\Debug
mklink /J Assets\Plugins\x86_64				%GIZMOSDK%\build\ws\vs15\GizmoSDK_Unity_Libs\x64\Debug
mklink /J Assets\Plugins\Android\Libs\armeabi-v7a 	%GIZMOSDK%\build\ws\vs15\GizmoSDK_Unity_Libs\ARM\Debug
mklink /J Assets\Plugins\Android\Libs\arm64-v8a 	%GIZMOSDK%\build\ws\vs15\GizmoSDK_Unity_Libs\ARM64\Debug
mklink /J Assets\Plugins\Android\Libs\x86 		%GIZMOSDK%\build\ws\vs15\GizmoSDK_Unity_Libs\x86\Debug


copy /Y %GIZMOSDK%\scripts\mono_settings\csc_32_DEBUG.rsp Assets\csc.rsp