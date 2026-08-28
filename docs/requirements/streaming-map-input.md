# Streaming Map input requirements

**Status:** Draft target requirements

## Purpose

CSWUnity consumes streamed scene data through the C# CSW SceneManager and
realizes that data in Unity. GZD is the first required map format; the
CSWUnity-facing scene contract must not prevent additional streamed formats.

## Functional requirements

### CSWU-MAP-001: GZD map input

CSWUnity shall load GZD maps generated and published by a supported version of
CSWMapGenerator through the C# CSW SceneManager and GizmoSDK.

### CSWU-MAP-002: Format ownership

GizmoSDK and CSW SceneManager shall own source-format resolution, parsing,
native scene traversal, and dynamic loading. CSWUnity shall not implement a
separate GZD parser.

### CSWU-MAP-003: Format-neutral Unity boundary

The scene-change contract consumed by CSWUnity shall describe common scene,
resource, coordinate, and lifecycle semantics without exposing GZD-specific
types to Unity realizers.

A future streamed format shall be addable below this boundary without
rewriting the CSWUnity frame scheduler or Unity object lifecycle.

### CSWU-MAP-004: Map addressing

CSWUnity shall accept map URLs supported by the configured CSW SceneManager and
GizmoSDK serialization adapters. Credentials and transport-specific settings
shall not be embedded in scene assets.

### CSWU-MAP-005: Map lifecycle

CSWUnity shall support asynchronous add, replace, remove, and clear operations
and expose their completion or failure.

When multiple maps are active, CSWUnity shall preserve the CSW SceneManager
coordinate-system compatibility rules and report incompatible maps.

### CSWU-MAP-006: Unity scene realization

CSWUnity shall process structural `New`, `Update`, `Delete`, and `Activation`
changes emitted by CSW SceneManager and realize supported content through a
configurable builder/factory registry.

### CSWU-MAP-007: Coordinate adaptation

CSWUnity shall preserve the Map Coordinate Context, the Map Origin
`GeoPosition` when georeferenced, Global 3D Positions, and Local 3D Frame
identity and generation while applying the current Unity Viewer Transform.

### CSWU-MAP-008: Dynamic streaming

CSWUnity shall submit camera and frame information to CSW SceneManager so that
GizmoSDK can update dynamic content and levels of detail without loading the
complete map into Unity.

### CSWU-MAP-009: Load failure reporting

Map and transport failures shall be returned as structured, correlated
diagnostics. Retry, replacement URL, and cancellation policy shall be owned by
the host application or an explicitly configured CSWUnity policy.

### CSWU-MAP-010: Version compatibility

Each supported release shall identify a validated combination of Unity,
CSWUnity, C# CSW SceneManager, GizmoSDK, and CSWMapGenerator versions.

## Acceptance criteria

- A published GZD map produced by the supported CSWMapGenerator version loads
  through the C# CSW SceneManager.
- Frame buffers produce correctly ordered Unity `New`, `Update`, `Delete`, and
  `Activation` results.
- Camera movement updates streamed content and levels of detail.
- Global 3D Positions remain stable across Local Origin Offset rebases.
- Invalid, unavailable, incompatible, and cancelled sources produce distinct
  observable results.
- A test format provider can emit the common scene contract without requiring
  changes to Unity builders that support its payload types.

## External sources

- [CSW architecture and SceneManager](https://saab.ghe.com/saab/CSW)
- [CSWMapGenerator](https://saab.ghe.com/saab/CSWMapGenerator)

## Related design

- [Streaming Map pipeline](../design/streaming-map-pipeline.md)
- [Next-generation architecture](../design/next-generation-architecture.md)
- [Coordinate nomenclature and mapping](coordinate-nomenclature.md)
