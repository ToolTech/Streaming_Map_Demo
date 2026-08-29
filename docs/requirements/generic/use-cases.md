# Generic CSW use cases

**Status:** Baseline

These use cases are non-normative stories describing expected use. Normative
statements are kept in [Generic CSW requirements](requirements.md).

## CSW-UC-001: Control a selected 3D engine through CSW

A host application uses the generic CSW API to control 3D functionality. The
same concepts are recognizable whether CSWUnity or CSWUnreal is selected.

## CSW-UC-002: Implement functionality in an engine

An engine developer implements a realization or hook inside CSWUnity or
CSWUnreal and registers it with CSW. Engine-native code can also use the
realization directly for an engine-only workflow.

## CSW-UC-003: Stream scene changes into an engine

SceneManager processes Gizmo3D scene changes asynchronously. The selected
engine receives buffered new, update, delete, frame, status, and response work,
then executes it through registered factories in its own update context.

## CSW-UC-004: Request and poll an asynchronous result

A caller submits a request with a correlation identity and continues working.
The result later becomes available through a callback, event, or polling API
using the same identity.

## CSW-UC-005: Work with a composed map

A host requests a map, adds or removes URL-based map parts, and works primarily
with the map as a whole. It can inspect projection, aggregate map information,
layers, drawing mode, and individual part metadata when needed.

## CSW-UC-006: Navigate a map

An Observer receives mouse or system input and uses a MotionModel such as map
orbit to move its Camera around a selected map. Its View shows what the Camera
sees.

## CSW-UC-007: Use another Camera for a View

SceneManager has a Main Camera describing the primary area of interest. A View
inside Unity or Unreal can use another Camera without moving the Main Camera.

## CSW-UC-008: Position a Camera across coordinate layers

A host positions and aims a Camera using generic geographic or global 3D
coordinates. The selected realization maps the same Camera to its current
engine coordinate system.

## CSW-UC-009: Work with world objects and events

A host accesses map objects, including entities and static objects, through an
Object API and reacts to updates and occurrences through an Event API.
