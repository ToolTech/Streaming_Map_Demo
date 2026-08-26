# External object integration requirements

**Status:** Draft target requirements

## Purpose

CSWUnity shall expose an engine-neutral object interface that makes it
straightforward for a host project to connect updates from CSW Distribution,
HLA, MCP, future DIS support, recorded scenarios, or product-specific systems.

Transport adapters and Unity presenters shall remain independent.

## Core object contract

### CSWU-OBJ-001: Stable identity

Every external object shall have a stable identity containing an identity
namespace and value. Unity instance IDs and transport-local pointers shall not
be public object identities.

### CSWU-OBJ-002: Type and schema

Every object shall identify a versioned type/schema. Schemas shall define
attribute names, value types, units, optionality, coordinate requirements, and
compatibility policy.

### CSWU-OBJ-003: Change types

The object contract shall distinguish:

- create;
- complete snapshot;
- partial attribute update;
- event;
- remove; and
- authority or ownership change.

### CSWU-OBJ-004: Dynamic values

The contract shall support typed dynamic attributes compatible with CSW and
GizmoSDK value semantics without forcing transport-specific encoding on
consumers.

### CSWU-OBJ-005: Time and revision

Every change shall carry available source time, receive time, source sequence
or revision, and correlation identity.

### CSWU-OBJ-006: Coordinate frame

Position, orientation, velocity, acceleration, and angular values shall declare
their coordinate frame, origin, axis convention, and SI units.

### CSWU-OBJ-007: Authority

The object service shall track update authority at object or attribute-group
level. Conflict resolution, leases/epochs, priorities, and write permission
shall be explicit policies.

## Feed, store, and adapter boundaries

### CSWU-OBJ-008: Transport-neutral feed

Transport adapters shall publish normalized object changes through an
engine-neutral feed with no `UnityEngine` dependency.

### CSWU-OBJ-009: Object store

An ordered object store shall validate lifecycle, schema, authority, revision,
and time before publishing approved changes to consumers.

### CSWU-OBJ-010: Transport adapters

The architecture shall support separate adapters for CSW Distribution, HLA,
MCP, recorded data, and future protocols. Adding an adapter shall not require
changes to Unity presenters.

### CSWU-OBJ-011: Bounded ingestion

Each external source shall use bounded queues or an explicit backpressure and
loss policy. Queue depth, dropped updates, reconnects, and resynchronization
shall be observable.

### CSWU-OBJ-012: Reconnect and snapshot

An adapter shall be able to re-establish state after reconnect through a
snapshot or equivalent resynchronization mechanism.

## Ordering and motion

### CSWU-OBJ-013: Duplicate and stale updates

The object store shall detect duplicate, stale, out-of-authority, and invalid
lifecycle updates and apply a documented policy.

### CSWU-OBJ-014: Reordering

Sources that permit out-of-order delivery shall support a bounded reorder
window based on source time and sequence.

### CSWU-OBJ-015: Motion presentation

Timestamped motion state shall support configurable interpolation, bounded
extrapolation, stale-state handling, and correction smoothing.

Motion estimation shall be a presentation service, not transport-specific
logic.

### CSWU-OBJ-016: Deterministic removal

Removal shall cancel pending motion and realization work before releasing the
Unity representation.

## Unity presentation

### CSWU-OBJ-017: Presenter registry

CSWUnity shall select a presenter through a deterministic registry based on
object schema and declared capabilities.

### CSWU-OBJ-018: Presenter lifecycle

A presenter shall support create, snapshot, delta, event, visibility, stale,
authority, and remove behavior as applicable.

### CSWU-OBJ-019: Main-thread boundary

Transport callbacks and object-store dispatch shall not mutate Unity objects.
Approved immutable state shall be applied through a bounded Unity main-thread
stage.

### CSWU-OBJ-020: Map alignment

External objects shall use the same world, coordinate, and local-origin
generation as streamed map content.

### CSWU-OBJ-021: Host extension

A host project shall be able to register product-specific object types,
schemas, presenters, and event handlers without forking CSWUnity.

## Acceptance criteria

- Equivalent object scenarios delivered through two adapters produce the same
  approved object-store state and Unity presentation.
- Create, partial update, event, remove, duplicate, out-of-order, reconnect,
  and authority-conflict cases have deterministic outcomes.
- A moving object remains aligned with UTM, geodetic, and geocentric maps.
- Transport callbacks perform no Unity access.
- Removal during queued interpolation or resource construction cannot publish
  a stale Unity object.

## Related design

- [External object architecture](../design/object-integration.md)
- [Scene control and threading](../design/scene-control-and-threading.md)
