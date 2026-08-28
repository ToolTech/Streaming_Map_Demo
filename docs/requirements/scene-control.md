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

The normative terms and conversion stages are defined in
[Coordinate nomenclature and mapping](coordinate-nomenclature.md).

### CSWU-COORD-001: Required coordinate modes

CSWUnity shall support at least:

- UTM Positions;
- Geodetic Positions; and
- Geocentric Positions.

Additional `GeoPosition` representations supported by GizmoSDK, including
general Projected Positions, MGRS References, and Flat-Earth Positions, shall
be addable through the same coordinate-service boundary.

### CSWU-COORD-002: Map Coordinate Context

For a georeferenced map, CSW SceneManager shall provide the CRS descriptor,
required metadata, Global 3D mapping, Map Origin `GeoPosition`, and stable
context identity or revision needed to establish the Map Coordinate Context
before dependent scene content is committed. A non-georeferenced Cartesian map
shall instead declare its explicit 3D frame and lack of geographic anchor.

The Map Origin shall remain a `GeoPosition`. Its mapped XYZ value shall be named
Origin Global 3D Position and shall not be passed or documented as though it
were the same value.

A transport may carry the derived Origin Global 3D Position for efficient scene
setup, but the field shall be named and typed accordingly and the Map Coordinate
Context shall retain the originating Map Origin `GeoPosition`.

### CSWU-COORD-003: Global 3D conversion

The coordinate service shall provide checked, bidirectional conversion between
supported `GeoPosition` representations and double-precision Global 3D
Positions in the active Map Coordinate Context.

CRS, datum, projection, height-model, and Global 3D mapping operations shall use
GizmoSDK coordinate services rather than duplicate formulas in viewer code.

### CSWU-COORD-004: Local 3D localization

The shared coordinate boundary shall provide checked, bidirectional
localization between:

- a double-precision Global 3D Position; and
- a single-precision Local 3D Position associated with an explicit Local 3D
  Frame identity and generation.

The Local 3D Frame shall declare its double-precision Local Origin Offset,
basis, and units. Translation-only localization shall compute the Global 3D
offset in double precision before converting the result to `float`.
The subtraction result is a displacement vector and shall be called a Local 3D
Position only after it has been expressed in the declared local frame.

### CSWU-COORD-005: Precision boundary

Numeric `GeoPosition` values, Global 3D Positions, Origin Global 3D Positions,
Local Origin Offsets, and all operands used for origin subtraction shall retain
double precision.

Conversion to Local 3D or viewer-native single precision shall occur only after
origin subtraction and any required double-precision basis or unit mapping.
The conversion shall reject non-finite or out-of-range values rather than
silently overflow.

### CSWU-COORD-006: Viewer mapping

Each viewer adapter shall define a bidirectional mapping between Local 3D
Position and one specifically named native target frame. The mapping shall
state its axis permutation, signs, handedness conversion, unit scale, native
precision, and root, level, actor, or component transform.

Generic names such as *Unity Position* or *Unreal Position* shall not be used
where World, Transform Local, Level, Actor Relative, or Component Relative
semantics differ.

### CSWU-COORD-007: Semantic transform operations

Coordinate and viewer adapters shall expose semantically distinct operations
for:

- positions;
- offset and direction vectors;
- normals; and
- orientations or bases.

Only position conversion shall apply the Local Origin Offset. Direction and
offset vectors shall use the applicable linear mapping, normals the normal
mapping, and orientations the basis mapping.

### CSWU-COORD-008: Local-frame rebasing

Changing a Local Origin Offset shall create a new Local 3D Frame generation.
The frame update and all dependent transforms shall become visible atomically.
Local 3D Positions from different identities or generations shall not be
combined.

Rebasing shall not modify the Map Origin, authoritative `GeoPosition` values,
Global 3D Positions, object identities, or core camera state.

### CSWU-COORD-009: Round trip and failure

Every supported forward conversion shall define its supported inverse,
tolerance, and failure behavior. Round trips through single-precision Local 3D
or viewer positions shall be validated against a published tolerance rather
than exact equality.

Unsupported coordinate types, missing metadata, stale frame generations, and
failed precision checks shall produce explicit failures and shall not expose a
partially converted position as valid.

### CSWU-COORD-010: Local tangent orientation

The coordinate service shall provide the Local Tangent Frame required for
heading, movement, normals, and orientation. ENU is a basis at a `GeoPosition`;
it is not synonymous with Local 3D Position.

In particular, subtracting two geocentric ECEF positions produces an
ECEF-axis offset and shall not be described as ENU without the required basis
rotation.

### CSWU-COORD-011: Mapping API semantics

Generic and templated conversion helpers shall identify their semantic source
and target frames at the API or policy level. Scalar conversion, component
permutation, handedness conversion, unit scaling, origin translation, and
native hierarchy transforms shall remain distinguishable in implementation and
tests.

Matching C++ or C# scalar and vector types shall not be treated as proof that
two values use the same position frame.

## Position and spatial-query control

### CSWU-POS-001: Camera state

CSWUnity shall submit the camera's Global 3D Position, orientation, projection,
viewport, LOD factor, and render time to CSW SceneManager in the active Map
Coordinate Context.

### CSWU-POS-002: Object positioning

The host shall be able to position a Unity object from a supported
`GeoPosition` or Global 3D Position and recover the corresponding Global 3D
Position and requested `GeoPosition` representation from a Unity object.

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
- Coordinate tests independently verify `GeoPosition` to Global 3D mapping,
  double-precision origin subtraction, Local 3D narrowing, and Unity mapping.
- Rebase tests prove that Global 3D Positions remain unchanged, Local 3D Frame
  generations change atomically, and no visible jump occurs.
- Position, offset, direction, normal, and orientation tests prove that
  translation is applied only to positions.
- Unsupported mappings, stale frame generations, and out-of-range float
  conversions fail explicitly.
- Camera, intersection, and ground-clamp operations remain responsive while
  map streaming and geometry preparation are active.
- Asset instances share immutable resources and receive independent transform,
  update, activation, and delete operations.
- A complete buffer sequence can rebuild the same logical scene in a headless
  test realization and the Unity realization.

## Related design

- [Scene control and threaded realization](../design/scene-control-and-threading.md)
- [Next-generation architecture](../design/next-generation-architecture.md)
