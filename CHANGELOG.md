# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

---

## [4.2]

### Fixed
* WHO	241209	TBD	(4.2)

---

## [4.0]

### Added
- new TerrainShader (with embedded support)
- Various performance/memory fixes for Foliage
- GizmoSDK 2.12.185.1
- Improved functionality for builders
- Added support for map assets (instancing)
- Resources are now released explicitly
- Removed instrumentation code
- Lots of generic fixes and improvements

## [1.2]
* AMO   230101 Updated gizmo to 2.12.143
* ALBNI 230101 Replaced old vertex/index Buffers with GraphicsBuffer to avoid copying data from gpu to cpu.

## [1.1]
* ALBNI 220101  Added a new FeatureMap that should be shared independent on loaded map.
* ALBNI 220101  New module (SkyModule) that handles skybox and more correct ambient light.
* AMO 220101    Updated gizmo to 2.13.132.
* AALBNI 220101  Fixed/improved occlusion culling for foliage.