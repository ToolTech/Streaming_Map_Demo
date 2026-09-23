# Requirements

This directory contains functional and non-functional requirements for
CSWUnity, the reusable Unity realization of the CSW Streaming Map presentation
capability.

## Requirements

- [Generic CSW baseline use cases](generic/use-cases.md)
- [Generic CSW baseline requirements](generic/requirements.md)
- [CSWUnity realization baseline](unity/requirements.md)
- [Use cases](use-cases.md)
- [Coordinate nomenclature and mapping contract](coordinate-nomenclature.md)
- [Next-generation architecture](next-generation-architecture.md)
- [Scene control](scene-control.md)
- [Synthetic world](synthetic-world.md)
- [External object integration](object-integration.md)
- [Movement models](movement-models.md)
- [Rendering](rendering.md)
- [Runtime initialization](runtime-initialization.md)
- [Streaming Map input](streaming-map-input.md)

## Authoring requirements

Use cases and requirements are separate artifacts. Use-case files describe
expected use as stories and contain no normative `shall` statements.
Requirement files contain measurable `shall` statements and link to relevant
use cases instead of embedding them. Requirements should avoid prescribing an
implementation unless a constraint originates in Unity, GizmoSDK C#, or the
surrounding CSW system.

Each requirement document should describe:

- purpose and scope
- functional requirements
- non-functional requirements
- Unity, GizmoSDK C#, and CSW constraints
- dependencies and assumptions
- acceptance criteria
- exclusions and open questions
