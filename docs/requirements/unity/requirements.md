# CSWUnity realization requirements

**Status:** Baseline

These requirements realize the [generic CSW requirements](../generic/requirements.md)
for Unity without adding new generic behavior.

## API levels

### CSWU-BASE-001: C# and Unity-native surfaces

CSWUnity shall expose the common capabilities through the generic C# CSW API,
implement the C# realization contracts, and keep Unity-native components and
extensions at the engine-native level. Unity developers shall be able to
implement and register a Unity-native realizer without changing generic CSW
code.

## SceneManager mechanism

### CSWU-SM-001: Receiver queue and Unity polling

The C# SceneManager receiver shall be callable from a SceneManager-owned thread
and shall only retain or copy, enqueue, and signal incoming buffers. A Unity
update shall fetch the queued buffers and execute Unity-affine work on the
Unity main thread.

### CSWU-SM-002: Typed and budgeted execution

CSWUnity shall process generic or response, error, frame, new, update, and
delete buffers in their received order. Frame and construction work shall have
configurable per-update budgets, and unfinished buffer work shall remain queued
for a later Unity update.

### CSWU-SM-003: Scene-instance identity

CSWUnity shall register and resolve each Unity realization by the source node
and PathID and shall use the parent source node and ParentPathID for hierarchy
operations. A Unity instance ID or object name shall not replace that identity.

### CSWU-SM-004: Factory preparation and Unity construction

CSWUnity shall select a registered realizer from the Gizmo3D node type and its
supported type hierarchy. Pre-build, update-preparation, and pre-destroy work
may run on a SceneManager thread when it uses no Unity API. Unity object
construction, mutation, registration, and destruction shall run on a Unity
execution context that permits those operations.

### CSWU-SM-005: New, update, and delete

A new command shall resolve the parent, create and build the Unity object, and
register its complete scene-instance identity. An update shall resolve and
update that object. A delete shall destroy the object and remove the identity
registration.

### CSWU-SM-006: Correlated result polling

CSWUnity shall preserve the command reference identity from an asynchronous
request to its response. A C# callback or event may publish the result, and a
polling operation shall return and consume the stored result for that identity.

## Other common capabilities

### CSWU-MM-001: MapManager realization

CSWUnity shall realize the generic whole-map MapManager operations and
metadata, including URL-based map parts, while keeping Unity scene objects out
of the generic MapManager API.

### CSWU-CV-001: Camera and coordinate realization

CSWUnity shall map generic GeoPosition, Global 3D, ENU, and object-local Camera
values to the selected Unity coordinate frame. A Unity View shall use its
referenced Camera even when it differs from SceneManager's Main Camera.

### CSWU-OE-001: Object and Event realization

CSWUnity shall expose the generic Object API and Event API through C# and shall
keep Unity object and event representations inside the Unity realization
layer.
