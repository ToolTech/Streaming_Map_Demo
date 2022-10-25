rem ---- DONT RUN THIS as a GIT USER, No need --------------------------------
rem ---- Set up all paths in the project as relative paths - Saab Dev Only ---

rmdir /S /Q Assets\Saab
mkdir Assets\Saab

rem ------------- runtime dlls -----------------------------------------------

REM rmdir /S /Q Assets\Plugins\x86
REM rmdir /S /Q Assets\Plugins\x86_64
rmdir /S /Q Assets\Plugins\Android\Libs

mkdir Assets\Plugins\Android\Libs

rem ............. plugin selections -----------------------------------------

REM mklink /J Assets\Plugins\x86                	%GIZMOSDK%\build\ws\vs16\GizmoSDK_Unity_Libs\Debug
REM mklink /J Assets\Plugins\x86_64                	%GIZMOSDK%\build\ws\vs16\GizmoSDK_Unity_Libs\x64\Debug
REM mklink /J Assets\Plugins\Android\Libs\armeabi-v7a 	%GIZMOSDK%\build\ws\vs16\GizmoSDK_Unity_Libs\ARM\Debug
REM mklink /J Assets\Plugins\Android\Libs\arm64-v8a     %GIZMOSDK%\build\ws\vs16\GizmoSDK\ARM64\Debug
REM mklink /J Assets\Plugins\Android\Libs\x86         	%GIZMOSDK%\build\ws\vs16\GizmoSDK_Unity_Libs\x86\Debug

mkdir Assets\Saab\Foundation
mkdir Assets\Saab\Utility


rem ---- Added a specific folder in Unity projects to reflect our architecture ---

mkdir Assets\Saab\Unity\Foundation
mkdir Assets\Saab\Unity\Utility

rem ------------- End of GizmoSDK dependencies -------------------

mklink /J Assets\Saab\Foundation\Saab.Foundation.Map.Manager 			%BTA%\source\foundation\Saab.Foundation.Map\Saab.Foundation.Map.Manager
mklink /J Assets\Saab\Utility\Saab.Utility.GfxCaps				%BTA%\source\utility\Saab.Utility.GfxCaps

rem ---------------- Link in Unity projects -----------------------------

mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.Mapstreamer 		%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.Mapstreamer
mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.Mapstreamer.Modules	%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.Mapstreamer.Modules
mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.NodeProperties 		%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.NodeProperties
mklink /J Assets\Saab\Unity\Utility\Saab.Utility.Unity.NodeUtils 			%BTA%\source\Utility\Unity\Saab.Utility.Unity.NodeUtils


rem ---------------- Shaders -------------------------------------------------

rmdir /S / Q Assets\Shaders
mklink /J Assets\Shaders 							%BTA%\resources\shaders


exit

