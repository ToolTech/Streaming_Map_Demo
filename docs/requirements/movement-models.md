# Movement model requirements

**Status:** Draft target requirements

## Purpose

CSWUnity shall provide interchangeable movement models for camera navigation
and other host-controlled rigs. Input, movement policy, map coordinates,
surface queries, constraints, and Unity camera application shall be separable.

## Common movement contract

### CSWU-MOVE-001: Separated responsibilities

The movement architecture shall separate:

- input source;
- movement model;
- coordinate service;
- surface/spatial query service;
- constraints;
- camera or object rig; and
- orchestration and timing.

### CSWU-MOVE-002: Input independence

A movement model shall consume an immutable input frame rather than call a
Unity input API directly.

### CSWU-MOVE-003: Deterministic step

A movement model shall advance from explicit state, input, time step,
coordinate context, and available query samples. The same inputs shall support
live operation, replay, and automated testing.

### CSWU-MOVE-004: Pose convention

The authoritative pose shall use a double-precision source position and a
documented orientation relative to an explicit local coordinate basis.
Conversion to a Unity transform shall occur at the rig boundary.

### CSWU-MOVE-005: Runtime selection

The host shall be able to select and change movement models through serialized
profiles or runtime control without replacing the CSW SceneManager.

### CSWU-MOVE-006: Constraints

Clearance, collision, slope, pitch, speed, acceleration, bounds, and other
policies shall be composable constraints rather than duplicated inside every
movement model.

### CSWU-MOVE-007: Async query use

Movement models shall not block the Unity main thread while waiting for ground,
building, or streamed-map data.

## Required models

### CSWU-MOVE-008: Free-flight model

CSWUnity shall provide a six-degree or configurable free-flight model with
explicit speed, acceleration, damping, local-up, and rotation policies.

### CSWU-MOVE-009: Geodetic orbit model

CSWUnity shall provide ellipsoid-aware pan, orbit, and zoom behavior using
local east/north/up orientation and a configurable focus or surface anchor.

### CSWU-MOVE-010: Flying-carpet model

CSWUnity shall provide a ground-following model that:

- maintains configurable target and minimum clearance;
- samples ground or selected surfaces asynchronously;
- supports smooth vertical and orientation response;
- can use local up or optional surface-normal alignment;
- defines speed, acceleration, and turn behavior; and
- applies a deterministic policy when surface data is unavailable.

### CSWU-MOVE-011: Unavailable-surface policy

Flying-carpet profiles shall select an explicit unavailable-data policy, such
as hold last valid surface, continue inertially within limits, climb to a safe
clearance, or stop.

## Extensibility

### CSWU-MOVE-012: Custom model

A host shall be able to add a movement model and parameter profile without
modifying the input, coordinate, query, or rig implementations.

### CSWU-MOVE-013: Input adapters

Live devices, Unity Input System actions, legacy input, network commands,
recordings, and automated tests shall be addable as input sources.

### CSWU-MOVE-014: Rig adapters

Movement models shall be usable with a Unity camera rig and with a
host-controlled object rig when their pose and constraint capabilities match.

### CSWU-MOVE-015: Object follow

The architecture shall allow an optional model or constraint that follows an
external world object while preserving coordinate and origin semantics.

## Diagnostics and tests

### CSWU-MOVE-016: Diagnostics

Movement diagnostics shall expose selected model, state, input age, query age,
clearance, active constraints, fallback state, and update cost.

### CSWU-MOVE-017: Test paths

Each supported model shall provide deterministic test paths and acceptance
tolerances for position, orientation, clearance, and coordinate transitions.

## Acceptance criteria

- Free-flight, geodetic orbit, and flying-carpet models use the same rig,
  coordinate, timing, and input contracts.
- A recorded input stream reproduces the same authoritative pose sequence
  within tolerance.
- Flying-carpet movement remains non-blocking while terrain tiles load and
  follows its configured unavailable-data policy.
- Switching movement models preserves pose without an unintended coordinate or
  origin jump.
- Movement remains correct on representative UTM, geodetic, and geocentric
  maps.

## Related design

- [Movement architecture](../design/movement-models.md)
- [Scene control](scene-control.md)
