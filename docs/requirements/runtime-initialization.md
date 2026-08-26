# Runtime initialization requirements

**Status:** Draft target requirements

## Purpose

CSWUnity must establish the GizmoSDK and C# CSW SceneManager runtime before
submitting map or frame commands. Initialization must be explicit, reusable,
and safe when CSWUnity is embedded in a larger Unity application.

## Functional requirements

### CSWU-INIT-001: Explicit composition

The host project shall be able to configure the CSWUnity runtime, camera,
streaming sources, realization profile, and optional features through explicit
component references and assets.

Runtime correctness shall not depend on scene-wide type searches or a
particular GameObject name or scene hierarchy.

### CSWU-INIT-002: Ordered platform startup

CSWUnity shall initialize the GizmoSDK platform before creating or starting a
C# CSW SceneManager.

Platform initialization shall be reference-counted or otherwise coordinated so
that multiple CSW components cannot initialize or shut down GizmoSDK out of
order.

### CSWU-INIT-003: SceneManager startup

CSWUnity shall:

1. create the C# CSW SceneManager;
2. register its command-buffer receiver;
3. start SceneManager processing;
4. submit initialization and capability commands; and
5. submit map commands only after initialization succeeds.

### CSWU-INIT-004: Lifecycle state

The runtime shall expose an observable lifecycle with at least `Created`,
`Starting`, `Running`, `Stopping`, `Stopped`, and `Failed` states.

Repeated start or stop requests shall be handled deterministically.

### CSWU-INIT-005: Configuration precedence

CSWUnity shall support serialized Unity configuration assets and explicit
runtime overrides supplied by the host application.

Configuration precedence shall be documented and shall not require a
checkout-relative file such as `config.xml`.

### CSWU-INIT-006: Diagnostics integration

CSWUnity shall route GizmoSDK and CSW SceneManager diagnostics to a configurable
Unity logging adapter while preserving severity, source, and correlation
information.

### CSWU-INIT-007: Controlled shutdown

Shutdown shall stop accepting new work, cancel or drain queued work according
to the selected shutdown policy, release Unity realization resources, detach
the buffer receiver, stop and dispose the C# CSW SceneManager, and finally
release the GizmoSDK platform reference.

No callback shall target destroyed Unity objects after shutdown begins.

### CSWU-INIT-008: Unity lifecycle independence

The host project shall be able to control runtime startup and shutdown
explicitly. Automatic `OnEnable`/`OnDisable` integration may be provided, but
it shall use the same public lifecycle and shall not be the only supported
mode.

### CSWU-INIT-009: Platform services

Platform-specific services, including Android registry, network, and multicast
setup, shall be isolated behind platform adapters. Projects that do not require
a platform service shall not have to include or activate it.

## Acceptance criteria

- A host project can configure and start CSWUnity without a sample scene or
  scene-wide object lookup.
- Map commands cannot reach SceneManager before platform and SceneManager
  initialization has completed.
- Startup failure produces the `Failed` state and a structured diagnostic.
- Starting or stopping twice does not leak resources or create duplicate
  workers.
- Explicit shutdown leaves no active SceneManager callback, worker, Unity
  object, or GizmoSDK platform reference owned by the stopped runtime.

## Related design

- [Next-generation architecture](../design/next-generation-architecture.md)
- [Runtime initialization design](../design/runtime-initialization.md)
