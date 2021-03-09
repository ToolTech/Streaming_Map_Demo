rem ---- DONT RUN THIS as a GIT USER, No need --------------------------------
rem ---- Set up all paths in the project as relative paths - Saab Dev Only ---

rmdir /S /Q Assets\Saab
mkdir Assets\Saab

rmdir /S /Q Tools
mkdir Tools

rem ------------- GizmoSDK dependencies --------------------------------------

if not defined GIZMOSDK (
  set GIZMOSDK=..\GizmoSDK
)

mkdir Assets\Saab\Foundation
mkdir Assets\Saab\Utility
mkdir Assets\Saab\Platform

mkdir Assets\Saab\Platform\GizmoSDK
mkdir Assets\Saab\Platform\GizmoSDK\Plugins


rem ---- Added a specific folder in Unity projects to reflect our architecture ---

mkdir Assets\Saab\Unity\Foundation
mkdir Assets\Saab\Unity\Utility

rem ----- Link in Gizmo as Utility -------------------------------------------------

mklink /J Assets\Saab\Platform\GizmoSDK\GizmoBase %GIZMOSDK%\GizmoBase\source\C#
mklink /J Assets\Saab\Platform\GizmoSDK\GizmoDistribution %GIZMOSDK%\GizmoDistribution\source\C#
mklink /J Assets\Saab\Platform\GizmoSDK\Gizmo3D %GIZMOSDK%\Gizmo3D\source\C#
mklink /J Assets\Saab\Platform\GizmoSDK\Plugins\Coordinate %GIZMOSDK%\plugins\gzCoordinate\source\C#

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


rem ---------------- Tools --------------------------------------------------

mklink /J Tools\SceneBuilder 							%BTA%\ws\vs16\foundation\Saab.Foundation.Map.Native\SceneBuilder\x64\Release
mklink /J Tools\SceneBuilderClient						..\BTA_Dev\GizmoSDK\utilities\SceneBuilderClient\x64\Release
mklink /J Tools\PurlServer							%GIZMOSDK%\GizmoBase\examples\purlserver\x64\Release



exit

