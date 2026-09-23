# Generic CSW requirements

**Status:** Baseline

This baseline contains only requirements explicitly discussed for the common
CSW 3D layer and the SceneManager mechanism reviewed in CSWUnreal. It does not
add unrelated retry, payload, persistence, test, or compatibility requirements.

## Documentation

### CSW-DOC-001: Separate use cases and requirements

Use cases and normative requirements shall be maintained in separate files.
Use cases shall describe expected use as stories, while requirements shall be
measurable statements.

## API levels

| Level | Responsibility |
| --- | --- |
| Generic CSW API | Engine-neutral concepts and logic used by a host through corresponding C++ and C# APIs |
| CSW realization API | Registered factories, hooks, buffers, and contracts used to realize generic capabilities in a selected engine |
| Engine-native API | Unity- or Unreal-native components and APIs used directly inside that engine |

### CSW-ARCH-001: Three API levels

CSW shall keep the generic CSW API, the CSW realization API, and the
engine-native API as distinguishable levels. Engine types shall not be required
by the generic CSW API.

### CSW-ARCH-002: Corresponding realizations

CSWUnity and CSWUnreal shall provide corresponding responsibilities and API
semantics for the generic capabilities through C# and C++ respectively. An
engine-native extension may add engine-specific behavior but shall not silently
change the meaning of a corresponding generic operation.

### CSW-ARCH-003: Control from either useful level

A host shall be able to control a selected engine through the generic CSW API.
An engine developer shall be able to implement or use an engine-native
realization without changing generic CSW code. Shared logic may reside in CSW
and invoke registered realization hooks, or a selected engine may provide the
complete implementation behind the same generic contract.

## SceneManager mechanism

The reviewed Unreal implementation provides the mechanism baseline:

```text
host command
  -> SceneManager input queue and worker processing
  -> traversal plus factory preparation
  -> typed output command buffers
  -> receiver-owned queue
  -> engine update polls and executes buffers
  -> engine object registry and correlated status/results
```

### CSW-SM-001: Typed asynchronous buffers

SceneManager shall communicate Gizmo3D scene changes and status through
asynchronous command buffers. The buffer contract shall distinguish generic or
response, error, frame, new, update, and delete work.

### CSW-SM-002: Receiver handoff and engine polling

A SceneManager receiver shall be callable from a SceneManager-owned thread and
shall be able to retain or copy, enqueue, and signal a received buffer. The
engine realization shall poll or fetch queued buffers and execute them in an
engine-approved update context rather than mutating engine objects in the
receiver callback.

### CSW-SM-003: Ordered and budgeted execution

The realization shall preserve buffer and command order while processing work.
It shall be able to limit frame and construction work per engine update and
leave unprocessed commands queued for a later update.

### CSW-SM-004: Scene-instance identity

New, update, activation, and delete commands shall identify a scene instance by
the source node together with its PathID. Hierarchy commands shall also carry
the parent source node and ParentPathID. The realization shall use that complete
identity when registering, finding, updating, and removing an engine object.

### CSW-SM-005: Factory and preparation boundary

The realization shall select registered factories from the Gizmo3D node type
and its supported type hierarchy. Pre-build, update-preparation, and
pre-destroy hooks shall be allowed to run under the SceneManager edit lock on a
SceneManager thread and shall return or retain the prepared realization data
needed by later engine execution without calling engine-thread-only APIs.

### CSW-SM-006: New, update, and delete lifecycle

A new command shall resolve its parent, create and build the engine object
through the selected factory, and register it under the complete scene-instance
identity. An update command shall resolve and update that same identity. A
delete command shall destroy the resolved engine object and remove its identity
registration.

### CSW-SM-007: Correlated requests and polling

An asynchronous request and its response shall carry the same command reference
identity. A realization shall be able to publish the response through its
native callback or event API and expose a polling operation that returns and
consumes the response for that identity.

### CSW-SM-008: Language surfaces

Unity shall consume this SceneManager mechanism through the C# CSW interface
and Unreal shall consume it through the C++ CSW interface.

## MapManager

### CSW-MM-001: Whole-map API

MapManager shall provide the public whole-map operations for requesting a map
and adding or removing URL-based map parts. Map and MapPart scene realizations
shall remain hidden behind SceneManager, while MapManager shall allow callers
to inspect map parts and their metadata.

### CSW-MM-002: Map information and control

MapManager shall expose the map CoordinateSystem from metadata, aggregate map
size, centroid, maximum LOD or view distance, planar, globe, or patch topology,
layer visibility and drawing policy, and map and view reset operations.

## Camera, View, Observer, and positions

### CSW-CV-001: Camera and View model

CSW shall support multiple Cameras and Views. A rendered View shall reference a
Camera and show what that Camera sees. A Camera shall describe the physical
camera model, position, direction, LookAt GeoPosition, and HeadUp policy. An
Observer shall combine a View, Camera, MotionModel, and input, including a map
orbit MotionModel around a selected map.

### CSW-CV-002: Coordinate levels and Main Camera

The generic Camera API shall use GeoPosition and, where the GizmoSDK mapping is
known, Global 3D Position. It shall support geodetic latitude, longitude, and
ellipsoid height, Cartesian positions, ENU, and object-local positions. Engine
realizations may use their native coordinate systems below this boundary.
SceneManager shall expose a Main Camera for the primary area of interest while
allowing an engine View to use another Camera.

## Objects and events

### CSW-OE-001: Object API and Event API

CSW shall expose an Object API and an Event API. Map objects shall be able to
represent entities and static objects. Their realization shall build on the
existing GizmoDistribution object and event mechanisms where applicable.

## Baseline constraints

### CSW-BASE-001: New target contract

The target CSW contracts shall not require backward compatibility with the
obsolete CSW implementation and shall not require changes to GizmoSDK core.
Existing implementations may be used as informative mechanism references.
