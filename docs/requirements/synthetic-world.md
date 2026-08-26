# Synthetic world requirements

**Status:** Draft target requirements

## Purpose

CSWUnity shall provide a Unity realization framework for the synthetic-world
capabilities defined by CSW. The framework must distinguish portable world
semantics from optional Unity rendering and effects.

The CSW capability taxonomy is broader than the currently implemented APIs.
CSWUnity shall not claim production support for a capability until its semantic
contract, data source, Unity realization, and validation profile are identified.

## Capability maturity

### CSWU-WORLD-001: Maturity levels

Each synthetic-world capability shall publish one of these maturity levels:

1. **Defined:** a versioned semantic contract exists;
2. **Connected:** at least one provider or adapter supplies the contract;
3. **Realized:** a Unity component or asset realizes it;
4. **Validated:** behavior and performance pass a published profile.

### CSWU-WORLD-002: Capability report

At runtime and in the Unity editor, CSWUnity shall report which capabilities
are defined, connected, realized, enabled, unsupported, or degraded.

## World context and time

### CSWU-TIME-001: Authoritative time

CSWUnity shall consume an explicit CSW world or simulation clock. Unity
`Time.time` shall not be assumed to be authoritative for simulation or
externally supplied state.

### CSWU-TIME-002: Time representation

World updates shall identify source time, receive time, clock identity, and
sequence where available. Simulation time and UTC time shall not be conflated.

### CSWU-TIME-003: Time control

The host shall be able to pause, resume, step, replay, or follow an external
clock when the selected CSW time provider supports those operations.

### CSWU-WORLD-003: Shared world context

Map, coordinate, object, environment, movement, query, sensor, and rendering
services shall use the same world identity, time context, coordinate context,
and origin generation.

## Environment contract

### CSWU-ENV-001: Environment provider

CSWUnity shall consume environment state through a versioned, engine-neutral
provider contract. The contract shall support snapshots or samples valid for a
specified time, position or region, coordinate frame, and altitude range.

### CSWU-ENV-002: Units and frames

Every environment value shall define units, coordinate frame, spatial
validity, temporal validity, source, quality, and authority where applicable.

### CSWU-ENV-003: Gravity

The environment contract shall represent gravity magnitude or vector and its
frame. A Unity physics realization shall document whether it follows this
value or uses a host override.

### CSWU-ENV-004: Wind

The environment contract shall represent at least a wind vector. It shall be
extensible to spatial fields, altitude layers, gusts, turbulence, uncertainty,
and forecast validity.

Unity wind visuals shall not become the authoritative wind used by CSW
simulation models.

### CSWU-ENV-005: Atmosphere

The environment contract shall be able to represent atmospheric temperature,
pressure, density, humidity, and derived values required by participating
simulation, sensor, or presentation models.

### CSWU-ENV-006: Visibility

The environment contract shall represent visibility or extinction information
with enough semantics for presentation and sensor consumers to apply the same
world state.

### CSWU-ENV-007: Precipitation

The environment contract shall support typed precipitation state, intensity,
phase, direction or fall velocity, and validity.

### CSWU-ENV-008: Clouds and sky

The environment contract shall support cloud and sky state without requiring
a particular Unity sky or volumetric-cloud implementation.

### CSWU-ENV-009: Lighting and time of day

The environment contract shall support the solar, lunar, ambient, and
time-of-day inputs required by installed lighting realizations.

### CSWU-ENV-010: Water and sea state

The environment contract shall support water level and optional wave, current,
surface, and sea-state information where provided by CSW.

### CSWU-ENV-011: Surface and vegetation state

The environment contract shall be extensible to surface condition, terrain
material, snow or wetness, vegetation state, and other map-related
environmental data.

### CSWU-ENV-012: Partial support

An environment provider may omit unsupported fields. Consumers shall declare
required and optional fields and apply a documented unavailable-data policy.

## Unity realization

### CSWU-WORLD-004: Optional realization modules

Sky, sun, clouds, fog, rain, snow, wind effects, water, vegetation,
post-processing, and sensor effects shall be optional Unity components/assets
selected through feature profiles.

### CSWU-WORLD-005: Render-pipeline adapters

Environment semantics shall remain independent of Unity render pipelines.
URP, HDRP, built-in, or future render-pipeline implementations shall be
replaceable adapters.

### CSWU-WORLD-006: Interpolation

Visual environment changes shall support configurable interpolation based on
world time. Interpolation shall not rewrite authoritative source state.

### CSWU-WORLD-007: Physical and visual separation

Physical models, sensor models, and visual effects may derive different outputs
from the same environment snapshot. Visual approximations shall be identified
and shall not feed back as authoritative simulation state unless explicitly
configured.

## Sensor integration

### CSWU-SENSOR-001: Sensor context

A synthetic sensor realization shall receive pose, world time, coordinate
context, and declared environment inputs.

### CSWU-SENSOR-002: Modality extensions

The architecture shall allow optional visual, infrared, radar, radio, and audio
presentation modules without adding those dependencies to the core map
streaming package.

### CSWU-SENSOR-003: Output metadata

Sensor outputs shall identify modality, source pose and time, projection or
field of view, resolution, and environment assumptions.

## Acceptance criteria

- A wind update can drive a CSW physical consumer and a Unity visual consumer
  from the same timestamped environment state.
- A recorded environment sequence replays deterministically at its semantic
  state boundary.
- Unsupported weather fields are reported without preventing map and object
  streaming.
- At least one selected release profile validates time, wind, visibility, and
  lighting integration.
- Installing the core package does not require a specific weather, water,
  cloud, or sensor rendering asset.

## External source

- [CSW capability architecture](https://saab.ghe.com/saab/CSW)

## Related design

- [Synthetic world architecture](../design/synthetic-world.md)
- [Rendering architecture](../design/rendering.md)
