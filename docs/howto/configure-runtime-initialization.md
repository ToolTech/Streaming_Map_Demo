# Configure runtime initialization

**Applies to:** Current implementation

This guide describes the existing Unity-owned `SceneManager`. It does not
describe the proposed C# CSW SceneManager architecture.

## Goal

Configure a Unity scene so CSWUnity initializes GizmoSDK and starts Streaming
Map loading.

## Prerequisites

- The CSWUnity package and its GizmoSDK native and managed dependencies are
  available to the Unity project.
- The scene contains a Unity camera with a concrete `CameraControlBase`
  component.
- A Streaming Map URL is available.

## Configure the scene

1. Create or select the GameObject that owns the Streaming Map runtime.
2. Add the `SceneManager` component.
3. Add the `Saab.Unity.Initializer.Initializer` component.
4. Assign the required node builders to `SceneManager.Builders`.
5. Set `SceneManager.MapUrl` to the default map URL.
6. Ensure exactly one active `CameraControlBase` implementation is available
   in the scene.
7. Provide `config.xml` at the local registry location resolved by GizmoSDK if
   the application requires local configuration.
8. Enter Play mode.

The initializer runs before components with the default Unity execution order.
It initializes GizmoBase, configures diagnostics and registries, resolves the
map URL, and connects the camera. `SceneManager.Start()` then initializes the
Gizmo3D streaming runtime and loads the map.

## Override the map URL

Set the GizmoSDK user key `SceneManager/MapUrl` to override the value stored in
the Unity scene. If the key is absent, the value in `SceneManager.MapUrl` is
used.

## Verify initialization

- The Unity console contains GizmoSDK startup messages.
- The `SceneManager` starts its dynamic loader manager.
- The configured map begins loading.
- No missing `SceneManager`, camera control, or node-builder errors are
  reported.

## Android

The initializer configures the default registry in the application's data
directory and acquires a Wi-Fi multicast lock. The application must have the
Android network permissions required by its deployment environment.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| The map does not load | Verify `SceneManager/MapUrl` and the Inspector fallback |
| The camera is not connected | Verify that an active `CameraControlBase` implementation exists |
| Geometry is missing | Verify the entries in `SceneManager.Builders` |
| GizmoSDK configuration is ignored | Verify that `config.xml` is available at the local registry location |
| Android discovery fails | Verify Wi-Fi state, permissions, multicast support, and the `wlan0` monitor configuration |

## Related documentation

- [Current architecture](../design/current-architecture.md)
- [Target runtime initialization](../design/runtime-initialization.md)
