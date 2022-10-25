rem ---- DONT RUN THIS as a GIT USER, No need --------------------------------
rem ---- Set up all paths in the project as relative paths - Saab Dev Only ---

rmdir /S /Q Assets\Saab
mkdir Assets\Saab


mkdir Assets\Saab\Foundation
mkdir Assets\Saab\Utility


rem ---- Added a specific folder in Unity projects to reflect our architecture ---

mkdir Assets\Saab\Unity\Foundation
mkdir Assets\Saab\Unity\Utility

rem ------------- End of GizmoSDK dependencies -------------------

mklink /J Assets\Saab\Foundation\Saab.Foundation.Map.Manager 	%BTA%\source\foundation\Saab.Foundation.Map\Saab.Foundation.Map.Manager
mklink /J Assets\Saab\Utility\Saab.Utility.GfxCaps				%BTA%\source\utility\Saab.Utility.GfxCaps

rem ---------------- Link in Unity projects -----------------------------

mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.Mapstreamer 			%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.Mapstreamer
mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.Mapstreamer.Modules	%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.Mapstreamer.Modules
mklink /J Assets\Saab\Unity\Foundation\Saab.Foundation.Unity.NodeProperties 		%BTA%\Source\Foundation\Unity\Saab.Foundation.Unity.NodeProperties
mklink /J Assets\Saab\Unity\Utility\Saab.Utility.Unity.NodeUtils 					%BTA%\source\Utility\Unity\Saab.Utility.Unity.NodeUtils


rem ---------------- Shaders -------------------------------------------------

rmdir /S / Q Assets\Shaders
mklink /J Assets\Shaders 							%BTA%\resources\shaders


exit

