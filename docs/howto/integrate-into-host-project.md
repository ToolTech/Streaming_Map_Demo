# Integrate CSWUnity into a host project

**Status:** Planned target workflow; not yet implemented

## Goal

Install CSWUnity in an existing Unity project, compose the required runtime
components and assets, and stream a published GZD map without depending on the
CSWUnity repository's example project.

This guide defines the intended consumer workflow. Final component, asset, and
menu names will be added when the target package API is implemented.

## Prerequisites

- A supported Unity and render-pipeline version.
- Access to the CSWUnity package and its compatible C# CSW SceneManager and
  GizmoSDK dependencies.
- A published GZD map from a compatible CSWMapGenerator version.
- A Unity camera selected by the host application.

## Install the package

1. Add the supported CSWUnity package source to Unity Package Manager.
2. Install the core CSWUnity package.
3. Install only the optional rendering-feature packages required by the host
   project.
4. Resolve the compatible native GizmoSDK plugins for the target platform.
5. Run the CSWUnity project validator and resolve all blocking compatibility
   errors.

No runtime script shall need to be copied from the CSWUnity sample project.

## Create the configuration

1. Create a runtime configuration asset.
2. Add one or more map-source entries and assign the published map URLs.
3. Create or select a realization profile containing the required
   builders/factories and their fallbacks.
4. Create or select a resource profile containing object, mesh, texture,
   material, queue, and per-frame budgets.
5. Add optional feature profiles for terrain detail, foliage, sensors, or
   pipeline-specific shaders.
6. Review the generated capability report for missing builders, assets,
   shaders, compute support, or render-pipeline incompatibilities.

Credentials and deployment-specific secrets are provided by the host
environment and are not stored in the configuration asset.

## Compose the Unity scene

1. Add the CSWUnity runtime-host component or supported prefab to a host-owned
   GameObject.
2. Assign the runtime configuration.
3. Assign the camera provider or explicit Unity camera.
4. Assign the coordinate/origin provider required by the host application.
5. Choose automatic Unity lifecycle integration or host-controlled startup.
6. Keep product-specific input, UI, and simulation components outside the
   CSWUnity runtime host.

CSWUnity shall not require a specific scene name, root name, singleton, or
sample hierarchy.

## Start streaming

1. Start the runtime through the selected lifecycle mode.
2. Wait for the observable `Running` state.
3. Add or enable the required map sources.
4. Submit camera updates and frame refreshes through the configured camera
   adapter.
5. Observe source status, queue depth, frame-apply cost, resource use, and
   errors through the diagnostics interface.

The C# CSW SceneManager performs map lifecycle, traversal, and dynamic loading.
CSWUnity applies completed scene changes to Unity under the configured
main-thread budget.

## Change maps at runtime

1. Submit an add, replace, remove, or clear request.
2. Retain the returned correlation identity.
3. Wait for completion or failure.
4. Handle coordinate-system incompatibility, cancellation, or transport
   failure through the host policy.

The host shall not edit the native GizmoSDK scene directly.

## Stop the runtime

1. Stop submitting camera and map commands.
2. Request runtime shutdown.
3. Wait for `Stopped` or a structured shutdown failure.
4. Destroy or unload the host scene only after CSWUnity has detached its
   SceneManager receiver and released Unity resources.

## Expected result

- The host project contains only supported CSWUnity package references,
  components, and serialized assets.
- A compatible GZD map streams according to the active camera.
- Optional features can be enabled or removed without changing the core
  streaming implementation.
- Streaming stays within the selected performance profile and reports overload
  rather than growing queues without limit.

## Related documentation

- [Next-generation architecture requirements](../requirements/next-generation-architecture.md)
- [Next-generation architecture design](../design/next-generation-architecture.md)
- [Streaming Map pipeline](../design/streaming-map-pipeline.md)
