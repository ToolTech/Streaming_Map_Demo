# Scene control requirements

**Status:** Draft target requirements

## Purpose

The next-generation C# CSW SceneManager interprets streamed maps through
GizmoSDK and translates scene state into buffer-based updates. CSWUnity
consumes those updates and realizes them in Unity.

The public control model must support both straightforward host integration
and advanced control of maps, coordinates, positions, intersections, assets,
and scene updates.

## Control levels

### CSWU-CTRL-001: Component-level control

A host project shall be able to configure common streaming behavior through
Unity components and serialized assets without handling CSW command buffers
directly.

### CSWU-CTRL-002: Service-level control

A host project shall have typed runtime services for map lifecycle,
coordinates, positions, spatial queries, assets, scene status, and
diagnostics.

### CSWU-CTRL-003: Extension-level control

Advanced integrations shall have a documented extension API for realization
factories, frame notifications, optional features, and correlated CSW
commands. Low-level buffer access shall be explicit and shall preserve
threading and ownership rules.

## Map control

### CSWU-MAPCTRL-001: Map lifecycle

The map service shall support asynchronous set, add, replace, remove, and clear
operations.

### CSWU-MAPCTRL-002: Map state

Each map source shall expose a stable source identity and observable states
including requested, loading, active, unloading, failed, and removed.

### CSWU-MAPCTRL-003: Map information

The map service shall expose available map metadata, including source URL,
coordinate system, origin, geographic extent, maximum LOD range, and format
information supplied by CSW SceneManager.

### CSWU-MAPCTRL-004: Multiple maps

When multiple maps are active, incompatible coordinate systems shall be
rejected with a structured error. Compatible maps shall remain independently
addressable.

### CSWU-MAPCTRL-005: Streaming controls

The host shall be able to configure loader count, pre-caching, LOD factor,
omnidirectional traversal, and supported source capabilities without accessing
the native GizmoSDK scene.

## Coordinate-system control

### CSWU-COORD-001: Required coordinate modes

CSWUnity shall support at least:

- UTM projected coordinates;
- geodetic latitude, longitude, and altitude; and
- Earth-centered geocentric coordinates.

Additional projected or local coordinate systems shall be addable through the
same coordinate-service boundary.

### CSWU-COORD-002: Coordinate description

CSW SceneManager shall provide CSWUnity with the coordinate-system descriptor
and map origin before Unity scene content depending on that context is
committed.

### CSWU-COORD-003: Bidirectional conversion

The coordinate service shall provide checked conversion between:

- source/map coordinates;
- geodetic coordinates;
- geocentric coordinates;
- UTM coordinates when defined by the active system; and
- Unity-local coordinates.

Conversions shall use GizmoSDK coordinate services rather than duplicate
projection formulas in CSWUnity.

### CSWU-COORD-004: Precision

Source, geodetic, geocentric, UTM, and world-origin values shall retain double
precision until conversion to a Unity representation that requires
single-precision values.

### CSWU-COORD-005: Local origin

CSWUnity shall support a configurable Unity-local origin and origin rebasing
without changing authoritative source coordinates or object identity.

### CSWU-COORD-006: Orientation

The coordinate service shall provide local up, north, and east directions and
the transformations required to convert positions, directions, normals, and
rotations correctly for each supported coordinate mode.

## Position and spatial-query control

### CSWU-POS-001: Camera state

CSWUnity shall submit camera position, orientation, projection, viewport, LOD
factor, and render time to CSW SceneManager using source-coordinate semantics.

### CSWU-POS-002: Object positioning

The host shall be able to position a Unity object from a map, geodetic,
geocentric, or UTM position and recover the authoritative world position from
a Unity object.

### CSWU-QUERY-001: Ray intersection

The spatial-query service shall support asynchronous ray intersection with a
configurable intersection mask and an option to wait for dynamic data.

### CSWU-QUERY-002: Ground clamp

The spatial-query service shall support asynchronous ground-clamp queries and
return success, position, normal, up direction, and altitude when available.

### CSWU-QUERY-003: Correlation and cancellation

Every asynchronous query shall return a correlation identity and support
timeout and cancellation without blocking the Unity main thread.

### CSWU-QUERY-004: Result identity

An intersection result shall identify the hit scene instance without exposing
an unmanaged pointer as the public identity.

## Asset and scene-update control

### CSWU-ASSET-001: Structural updates

CSW SceneManager shall emit typed `New`, `Update`, and `Delete` changes with
stable instance and parent identities.

### CSWU-ASSET-002: Activation updates

LOD and streaming visibility changes shall be emitted as activation updates
without requiring geometry recreation.

### CSWU-ASSET-003: Asset instances

Referenced or instanced assets shall preserve shared resource identity while
allowing independent transforms, hierarchy positions, and activation state.

### CSWU-ASSET-004: Resource changes

Geometry, material, texture, metadata, and transform changes shall be
distinguishable so that a realizer can update only the affected Unity
resources.

### CSWU-ASSET-005: Extensible payloads

Scene payloads shall support typed optional extensions for future streamed
formats and feature data without changing the common frame and lifecycle
protocol.

### CSWU-SCENE-001: Typed buffers

The managed CSW contract shall expose at least `Generic`, `Error`, `Frame`,
`New`, `Update`, and `Delete` buffers and all commands required to consume
them.

### CSWU-SCENE-002: Complete frames

Frame buffers shall include correlated start and end boundaries. CSWUnity
shall not publish a partially committed frame to dependent modules.

### CSWU-SCENE-003: Ordered hierarchy

Scene updates shall be applicable in deterministic dependency order, including
parent-before-child creation and child-before-parent deletion where required.

### CSWU-SCENE-004: Thread-safe transfer

SceneManager callbacks shall transfer buffer work without invoking Unity APIs
and without retaining unlocked mutable native references.

## Acceptance criteria

- The C# API can consume all structural, frame, geographic, error, intersection,
  and ground-clamp commands used by the CSWUnreal reference flow.
- A host can load, add, replace, remove, and clear GZD maps through the typed
  map service.
- UTM, geodetic, and geocentric test maps produce correct Unity positions,
  orientations, and round-trip conversions.
- Camera, intersection, and ground-clamp operations remain responsive while
  map streaming and geometry preparation are active.
- Asset instances share immutable resources and receive independent transform,
  update, activation, and delete operations.
- A complete buffer sequence can rebuild the same logical scene in a headless
  test realization and the Unity realization.

## Related design

- [Scene control and threaded realization](../design/scene-control-and-threading.md)
- [Next-generation architecture](../design/next-generation-architecture.md)
