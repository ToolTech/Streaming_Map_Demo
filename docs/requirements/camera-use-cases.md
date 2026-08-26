# Camera and movement use cases

**Status:** Draft CSWUnity SDK use cases

## Scope

These use cases define reusable camera and movement behavior for CSWUnity.
They do not prescribe input bindings, application UI, selection behavior, or a
specific Unity camera rig.

The host project maps mouse, keyboard, touch, controller, network, replay, or
other input to semantic operations. CSWUnity owns the map-relative movement,
coordinate, query, constraint, transition, and outcome contracts.

## Actors

| Actor | Responsibility |
| --- | --- |
| User | Navigates and inspects the synthetic world |
| Host | Supplies input, viewport, configuration, targets, and lifecycle |
| Camera service | Executes semantic camera and movement operations |
| Map service | Supplies extent, coordinate context, lifecycle, and queries |
| Scene service | Consumes the active camera for streaming, culling, and LOD |

## Map lifecycle and framing

### CSWU-UC-CAM-001: Establish the initial map view

**Preconditions:** Stable map extent and coordinate metadata are available.

**Flow**

1. The map becomes ready.
2. The camera service centers and fits the metadata extent.
3. Local north is placed at the top of the view.
4. The configured initial tilt is applied.

**Outcome:** The initial view is available immediately and does not wait for
all streamed objects or reframe as content arrives.

### CSWU-UC-CAM-002: Fit the active map

The host requests Fit Map. The camera frames the stable metadata extent inside
the supplied usable viewport and padding while preserving current heading and
tilt.

### CSWU-UC-CAM-003: Return to a host-defined home view

The host requests Home. The camera returns to the configured home pose. Home
and Fit Map remain separate semantic actions.

### CSWU-UC-CAM-004: Preserve view across viewport changes

When viewport dimensions or usable bounds change, the camera preserves the
current focus and visible scale as closely as the new aspect ratio permits.

### CSWU-UC-CAM-005: Handle map loss or replacement

When the active map unloads or is replaced, map-dependent actions and tracking
stop. Requests return an unavailable outcome until compatible metadata for a
new map is ready.

## Manual map navigation

### CSWU-UC-CAM-010: Pan a projected map

The user pans a flat or projected map. The grabbed map point remains under the
input anchor while heading, tilt, and zoom are preserved. Navigation is not
implicitly clamped to map extent.

### CSWU-UC-CAM-011: Pan a spherical map

The user pans a geodetic or geocentric map as a globe. The local heading,
tilt, and zoom are preserved across poles and coordinate seams without jumps,
unexpected roll, or reversed controls.

### CSWU-UC-CAM-012: Zoom around an anchor

When an input anchor resolves to the map, zoom preserves that point on screen.
Otherwise, the camera uses the current viewport focus or last valid focus.
Zoom rate scales with view distance and respects configured limits.

### CSWU-UC-CAM-013: Orbit around a map focus

The user changes heading and tilt around the current focus. The view remains
upright. If no current intersection exists, the last valid focus is used; if
none exists, the operation reports unavailable.

### CSWU-UC-CAM-014: Reset local north

The host requests Reset North. The camera changes only heading so that local
geographic north appears at the top of the viewport. Focus, distance, and tilt
remain unchanged, including on spherical maps near poles or seams.

### CSWU-UC-CAM-015: Navigate in free flight

The host selects free flight. The user can translate and rotate in three
dimensions according to the active speed, acceleration, damping, local-up,
and constraint policies.

### CSWU-UC-CAM-016: Stop manual movement

When movement input ends, the movement model follows its declared stop policy.
The default direct-navigation profile stops immediately; inertia is provided
only by a model or continued host input that explicitly requests it.

## Focus and semantic movement

### CSWU-UC-CAM-020: Focus bounds

The host supplies one or more valid bounds in map coordinates. The camera
centers and frames their combined extent inside the usable viewport while
preserving heading and tilt.

### CSWU-UC-CAM-021: Focus a position

The host supplies a valid map-relative position. It becomes the current focus,
while distance is preserved unless the request supplies a target distance.

### CSWU-UC-CAM-022: Look, move, or fly to a target

The host requests a semantic look-at, move-to, or fly-to operation using map
coordinates. The operation applies the selected transition and active
constraints without requiring Unity world coordinates.

On a spherical map, long-distance movement follows a continuous valid path and
does not pass through the reference ellipsoid.

### CSWU-UC-CAM-023: Resolve invalid targets

If bounds, position, focus, or path cannot be resolved, the camera remains at
its current valid pose and reports unavailable rather than applying a partial
movement.

## Constraints and ground following

### CSWU-UC-CAM-030: Apply a movement constraint profile

The host selects a constraint profile. Its zoom, clearance, collision, slope,
pitch, speed, acceleration, and bounds rules apply consistently to manual and
programmatic movement.

### CSWU-UC-CAM-031: Move along a constraint boundary

When requested motion crosses a boundary, the camera stops at the boundary
without jumping and may continue along it when the remaining movement permits.

### CSWU-UC-CAM-032: Recover from an invalid pose

When a stricter constraint profile is enabled while the current pose is
invalid, the camera resolves the nearest valid pose according to the profile
and reports the correction.

### CSWU-UC-CAM-033: Follow terrain as a flying carpet

The user moves over the map while the model maintains configured clearance
through asynchronous surface queries. Height and optional orientation changes
are smoothed.

If surface data is unavailable, the configured hold, limited-inertial, safe
climb, or stop policy is applied without blocking the Unity main thread.

## Transitions and outcomes

### CSWU-UC-CAM-040: Select immediate or animated movement

The host chooses whether a discrete action is immediate or animated. Initial
map framing is immediate unless a future profile explicitly defines otherwise.

### CSWU-UC-CAM-041: Interrupt an active transition

Manual movement, an explicit cancellation, a newer request, map removal, or
target invalidation cancels the active transition. The current valid pose is
preserved.

### CSWU-UC-CAM-042: Observe action outcome

Every discrete or animated request reports a correlation identity and one of
completed, cancelled, unavailable, or failed.

## Target tracking

### CSWU-UC-CAM-050: Track a moving target

The host starts tracking a map-relative target. The camera preserves configured
target framing, distance, heading, and tilt using immediate or smoothed
tracking.

### CSWU-UC-CAM-051: Stop tracking

Manual navigation, another discrete action, target removal, or an unreachable
constraint state stops tracking at the last valid pose and reports the reason.
A host profile may request a subsequent Home action.

### CSWU-UC-CAM-052: Follow in a target-relative frame

The host selects attached tracking. The camera preserves a configurable
target-relative pose and optional interactive adjustment while the target
moves and rotates.

## Streaming integration and observation

### CSWU-UC-CAM-060: Navigate during streaming

Navigation remains responsive after stable map metadata is ready while detail
continues to stream. New and removed content does not implicitly move or
reframe the camera.

### CSWU-UC-CAM-061: Preserve pose during origin rebasing

When the streamed scene changes local origin, the authoritative map-relative
camera pose remains unchanged and the user observes no jump.

### CSWU-UC-CAM-062: Drive scene streaming

The camera service publishes a coherent position, orientation, projection,
viewport, LOD, and render-time snapshot for CSW SceneManager traversal.

### CSWU-UC-CAM-063: Observe camera state

The host can observe authoritative pose, focus, heading, tilt, distance,
movement model, transition state, and active constraints without taking
ownership of camera state.

## Deferred candidates

The following remain candidates rather than initial requirements:

- multiple simultaneous cameras or viewports;
- runtime perspective/orthographic switching;
- saved views and bookmarks;
- scripted paths, tours, and recordings;
- cross-process camera synchronization;
- fixed-angle heading-step actions; and
- dedicated top-down navigation.

## Traceability

- [Movement model requirements](movement-models.md)
- [Scene control requirements](scene-control.md)
- [Movement architecture](../design/movement-models.md)
