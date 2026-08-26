# CSWUnity use-case catalog

**Status:** Draft target use cases

## Purpose

This is the independent use-case catalog for the low-level CSWUnity SDK and
Unity component package. It provides the behavioral basis for requirements,
design, and acceptance tests.

Use cases may be derived from experience in consuming systems, but this catalog
does not retain their component names, backlog identifiers, UI concepts, or
product architecture.

## Inclusion rules

A use case belongs in this catalog when it:

- describes reusable CSWUnity behavior;
- can be exercised through a public component, service, or extension contract;
- does not require product-specific editing, selection, undo, UI, or workflow
  concepts; and
- can produce implementation-independent acceptance criteria.

Application-specific controls remain the responsibility of the host project.
The host maps them to semantic CSWUnity operations.

## Domain catalogs

- [Camera and movement](camera-use-cases.md)

## Actors

| Actor | Goal |
| --- | --- |
| Operator | Explore and interact with a large synthetic world |
| Application programmer | Integrate CSWUnity into a Unity product |
| Feature programmer | Add data sources, realizers, environment features, or movement models |
| System integrator | Connect CSWUnity to external CSW or simulation systems |
| Tester | Verify behavior, coordinates, rendering, performance, and failure handling |
| External system | Publish world objects, events, environment state, or time |

## Operator use cases

### CSWU-UC-MAP-001: Explore a streamed map

**Preconditions**

- CSWUnity is running in a host Unity project.
- A compatible GZD map is available.
- A camera and movement profile are selected.

**Main flow**

1. The operator opens the map.
2. CSW SceneManager interprets and streams the map.
3. CSWUnity realizes visible terrain, geometry, materials, and assets.
4. The operator moves the camera.
5. LOD and dynamic content change without loading the complete map at once.

**Expected outcome**

The map remains visually coherent and responsive within the active performance
profile.

### CSWU-UC-COORD-001: Work in different map coordinate systems

1. The operator or application opens a UTM, geodetic, or geocentric map.
2. CSWUnity selects the corresponding coordinate adapter.
3. Camera, world objects, spatial queries, and shader origin data use the same
   coordinate context.
4. The operator views positions in the coordinate representation required by
   the host application.

**Expected outcome**

Map content and world objects align, and round-trip position conversion remains
within the configured tolerance.

### CSWU-UC-MOVE-001: Navigate with a flying-carpet model

1. The operator selects the ground-following movement profile.
2. The operator supplies forward, lateral, vertical-clearance, and look input.
3. The movement model requests surface information asynchronously.
4. The camera follows terrain at the configured clearance and smoothing.
5. If surface data is unavailable, the configured fallback policy is applied.

**Expected outcome**

The camera moves smoothly without blocking the Unity main thread or crossing
the configured minimum clearance.

### CSWU-UC-ENV-001: Observe changing weather and wind

1. An authoritative environment source updates time, wind, visibility, cloud,
   precipitation, or other available environment state.
2. Physical, object-motion, sensor, and presentation consumers receive
   timestamped state from the same environment context.
3. Installed Unity feature modules update their visual realization.

**Expected outcome**

The synthetic world changes coherently. Unsupported visual features report a
capability gap rather than changing simulation semantics.

### CSWU-UC-OBJ-001: Observe externally controlled objects

1. An external system creates one or more typed world objects.
2. It publishes timestamped state updates and events.
3. CSWUnity maps each object to a registered Unity presenter.
4. Motion updates are interpolated or extrapolated according to policy.
5. Removal releases the Unity representation and shared resources.

**Expected outcome**

Objects move smoothly, preserve identity, and remain consistent across delayed,
duplicate, or partial updates.

### CSWU-UC-QUERY-001: Query the synthetic world

1. The operator or application submits a ray intersection or ground-clamp
   request.
2. CSW SceneManager evaluates the streamed scene and dynamic data.
3. CSWUnity returns a correlated result containing position, normal, up,
   distance or altitude, and stable object identity where applicable.

**Expected outcome**

The request is asynchronous, cancellable, and does not stall rendering.

## Programmer use cases

### CSWU-UC-PKG-001: Integrate CSWUnity into a host project

1. The programmer installs supported Unity packages.
2. The programmer creates runtime, realization, resource, movement, and
   optional feature profiles.
3. The programmer adds a runtime host and supplies explicit camera and
   lifecycle references.
4. The project starts without depending on the CSWUnity sample scene.

### CSWU-UC-OBJ-002: Connect an external object system

1. The programmer implements or configures a transport adapter.
2. The adapter maps external identity, type, time, authority, coordinates,
   attributes, and events into the CSW object contract.
3. The object store validates and orders updates.
4. Existing Unity presenters consume the normalized object state.

**Expected outcome**

Transport code does not depend on Unity objects, and presenters do not depend
on HLA, MCP, Distribution, DIS, or another transport.

### CSWU-UC-OBJ-003: Add a world-object presenter

1. The programmer declares the object schema and presentation capabilities.
2. The programmer implements create, update, visibility, and release behavior.
3. The presenter is registered in a realization profile.
4. Validation detects missing dependencies or ambiguous selectors.

### CSWU-UC-ENV-002: Add an environment realization

1. The programmer selects an environment state field such as wind, fog,
   clouds, precipitation, water, lighting, or vegetation state.
2. The programmer implements a render-pipeline-specific Unity module.
3. The module declares required semantic inputs and hardware capabilities.
4. The module subscribes to environment snapshots and frame/origin updates.

### CSWU-UC-MOVE-002: Add a movement model

1. The programmer implements the movement-model contract without reading Unity
   input directly.
2. The model consumes immutable input, coordinates, time, and optional surface
   samples.
3. A profile exposes model parameters.
4. The same model runs with live input, recorded input, and automated tests.

### CSWU-UC-FORMAT-001: Add a future streamed format

1. GizmoSDK or CSW SceneManager gains support for the source format.
2. The format produces the common scene and resource payloads.
3. Existing compatible CSWUnity realizers continue to work.
4. Format-specific metadata is handled by optional typed extensions.

## Tester use cases

### CSWU-UC-TEST-001: Replay a deterministic scenario

The tester replays recorded map, object, environment, time, and input updates
and verifies the same logical scene state, object transforms, environment
snapshots, and query results.

### CSWU-UC-TEST-002: Verify coordinate systems

The tester runs representative UTM, geodetic, and geocentric maps through
known control points, camera paths, intersections, ground clamps, origin
rebases, and round-trip conversions.

### CSWU-UC-TEST-003: Verify streaming and rendering performance

The tester runs a published camera path and external-object load against a
versioned map and environment scenario. Automated gates compare frame time,
main-thread streaming time, latency, queue depth, memory, allocation, visible
content, and resource reuse with the release profile.

### CSWU-UC-TEST-004: Verify overload and failure handling

The tester injects unavailable maps, malformed updates, connection loss,
out-of-order messages, queue overload, missing shaders, unsupported
capabilities, cancelled queries, and shutdown during active loading.

### CSWU-UC-TEST-005: Verify visual realization

The tester captures approved viewpoints and environment states for supported
render pipelines and compares geometry, material, lighting, weather,
visibility, object placement, and LOD behavior against tolerances or approved
references.

## Traceability

| Use-case group | Primary requirements |
| --- | --- |
| Map exploration and coordinates | [Scene control](scene-control.md), [Streaming Map input](streaming-map-input.md) |
| Synthetic environment | [Synthetic world](synthetic-world.md) |
| External objects | [External object integration](object-integration.md) |
| Camera and flying carpet | [Movement models](movement-models.md) |
| Efficient presentation | [Rendering](rendering.md) |
| Package integration | [Next-generation architecture](next-generation-architecture.md), [Runtime initialization](runtime-initialization.md) |
