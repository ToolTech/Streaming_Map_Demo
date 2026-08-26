# Rendering requirements

**Status:** Draft target requirements

## Purpose

CSWUnity shall render large streamed maps, external world objects, and optional
synthetic-environment effects efficiently and coherently in Unity.

## Rendering model

### CSWU-RENDER-001: Unified world frame

Map geometry, external objects, environment effects, spatial queries, and
camera state shall use the same committed world time, coordinate context, and
local-origin generation.

### CSWU-RENDER-002: Representation strategies

The realization registry shall support multiple Unity representation
strategies, including:

- GameObjects/components for interactive semantic objects;
- shared meshes and materials for reusable assets;
- GPU instancing or indirect rendering for dense repeated content;
- batched or data-oriented representations for high-volume content; and
- render-pipeline-specific passes for optional effects.

### CSWU-RENDER-003: Strategy selection

Representation selection shall be based on declared payload and rendering
capabilities, not hard-coded native node type checks in a monolithic manager.

### CSWU-RENDER-004: Frame coherence

Rendering modules shall observe only committed map, object, environment, and
origin state. Partial scene-buffer application shall not be visible to
dependent modules.

## Culling, LOD, and visibility

### CSWU-RENDER-005: Source LOD

CSW SceneManager and GizmoSDK shall retain responsibility for streamed-map
traversal and source LOD.

### CSWU-RENDER-006: Unity culling

CSWUnity shall support Unity frustum, layer, shadow, occlusion, and
renderer-specific culling without breaking CSW activation semantics.

### CSWU-RENDER-007: Dense feature culling

Dense features such as foliage shall support GPU or otherwise scalable
placement, culling, and drawing where required by the release profile.

### CSWU-RENDER-008: Activation without rebuild

LOD or streamed visibility changes shall not rebuild immutable geometry or
resources when activation alone is sufficient.

## Resources and threading

### CSWU-RENDER-009: Shared resources

Meshes, textures, materials, shaders, and referenced assets shall use explicit
sharing, leases, pooling, caching, budgets, and eviction.

### CSWU-RENDER-010: Threaded preparation

Pure geometry, image, hierarchy, and batching preparation shall be eligible for
workers, Unity Jobs, or Burst. Final Unity publication shall follow supported
thread-affinity rules.

### CSWU-RENDER-011: Bounded publication

Main-thread resource and hierarchy publication shall use configurable time and
item budgets with stale-work cancellation.

### CSWU-RENDER-012: Allocation

Steady-state streaming within pre-sized capacities shall produce no ordinary
managed garbage allocation on the Unity main thread after warm-up.

### CSWU-RENDER-013: Upload control

Geometry and texture paths shall minimize redundant native/managed/GPU copies
and expose upload volume and latency.

## Map and synthetic-world features

### CSWU-RENDER-014: Terrain and asset baseline

The baseline realization shall support the terrain, static geometry, material,
texture, transform, hierarchy, reference-instance, update, activation, and
delete payloads required by the selected GZD compatibility profile.

### CSWU-RENDER-015: Optional environment modules

Weather, clouds, fog, precipitation, water, vegetation, lighting, and sensor
effects shall be optional modules consuming common world/environment state.

### CSWU-RENDER-016: Quality profiles

The host shall be able to select a quality profile controlling loaders, LOD,
shadows, texture and mesh budgets, dense-feature density, environment effects,
sensor effects, and main-thread/GPU budgets.

### CSWU-RENDER-017: Graceful degradation

When hardware, render-pipeline, shader, compute, or budget capabilities are
missing, optional modules shall disable or select a documented fallback without
invalidating core map and object streaming.

### CSWU-RENDER-018: Large-world precision

Rendering shall support local-origin rebasing and camera-relative shader data
while preserving double-precision authoritative positions.

## Validation

### CSWU-RENDER-019: Performance profile

Every validated rendering configuration shall define map, object count,
environment state, camera path, hardware, Unity/render-pipeline version,
quality settings, frame target, CPU/GPU budgets, latency, memory, and allocation
limits.

### CSWU-RENDER-020: Rendering metrics

Metrics shall include source and Unity culling counts, visible instances,
prepared/published/stale work, draw calls, batches, triangles, texture and mesh
memory, cache behavior, upload volume, main-thread time, worker time, GPU time,
and end-to-end visibility latency.

### CSWU-RENDER-021: Visual tests

Supported profiles shall include controlled visual tests for geometry,
materials, LOD, shadows, origin shifts, weather, lighting, external objects,
and optional sensor modes.

### CSWU-RENDER-022: Stress tests

Stress tests shall cover rapid camera movement, map replacement, dense external
updates, environment transitions, queue overload, asset churn, origin rebasing,
and shutdown during active work.

## Acceptance criteria

- Representative maps and synthetic-world scenarios meet their published CPU,
  GPU, memory, allocation, and latency profiles.
- Map, object, environment, and camera state remain spatially and temporally
  coherent.
- Optional rendering modules can be removed without changing core map or
  external-object contracts.
- Stale map or object work cannot become visible after delete, replacement, or
  origin-generation change.

## Related design

- [Rendering architecture](../design/rendering.md)
- [Scene control and threaded realization](../design/scene-control-and-threading.md)
- [Synthetic world architecture](../design/synthetic-world.md)
