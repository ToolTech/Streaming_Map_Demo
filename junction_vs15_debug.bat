rem ---- DONT RUN THIS as a GIT USER, No need --------------------------------
rem ---- Set up all paths in the project as relative paths - Saab Dev Only ---


rem ------------- GizmoSDK dependencies --------------------------------------

if not defined GIZMOSDK (
  set GIZMOSDK=..\GizmoSDK
)

rmdir /S / Q Assets\Saab\GizmoSDK
mkdir Assets\Saab\GizmoSDK
mkdir Assets\Saab\GizmoSDK\Plugins
mklink /J Assets\Saab\GizmoSDK\GizmoBase %GIZMOSDK%\GizmoBase\source\C#
mklink /J Assets\Saab\GizmoSDK\GizmoDistribution %GIZMOSDK%\GizmoDistribution\source\C#
mklink /J Assets\Saab\GizmoSDK\Gizmo3D %GIZMOSDK%\Gizmo3D\source\C#
mklink /J Assets\Saab\GizmoSDK\Plugins\Coordinate %GIZMOSDK%\plugins\gzCoordinate\source\C#

rem ------------- End of GizmoSDK dependencies -------------------------------

rmdir /S / Q Assets\Saab\Saab.Map
mklink /J Assets\Saab\Saab.Map ..\BTA\source\foundation\Saab.Map

rem ------------- runtime dlls -----------------------------------------------

rmdir /S / Q Assets\Plugins
mkdir Assets\Plugins

mklink /J Assets\Plugins\x64 ..\BTA\ws\vs15\BTA\x64\Debug

copy /Y ..\BTA\resources\mono_settings\mcs_WIN64_DEBUG.rsp Assets\mcs.rsp