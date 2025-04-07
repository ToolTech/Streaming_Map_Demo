# Changelog MapStreamer

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
- TBD

## 4.1

### Added
- Shared materials
- Simple water shading

### Changed
- Improved foliage culling
- Better random function in shader

### Fixed
- hueshift breaking at white/black
- Wrong foliage placement between Lods

## 4.0

### Added
- new TerrainShader (with embedded support)
- Added support for map assets (instancing)

### Removed
- Removed instrumentation code

### Changed
- GizmoSDK 2.12.185.1
- Resources are now released explicitly

### Fixed
- Various performance/memory fixes for Foliage
- Improved functionality for builders
- Lots of generic fixes and improvements

## 1.2

### Changed
- Updated gizmo to 2.12.143
- Replaced old vertex/index Buffers with GraphicsBuffer to avoid copying data from gpu to cpu.

## 1.1

### Added
- Added a new FeatureMap that should be shared independent on loaded map.
- New module (SkyModule) that handles skybox and more correct ambient light.

### Changed
- Updated gizmo to 2.13.132.

### Fixed
- Fixed/improved occlusion culling for foliage.