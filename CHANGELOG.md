# Changelog

All notable changes to this project shall be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
described with adjustments and template in [TSWiki](https://tswiki.corp.saab.se/CHANGELOG.md_Windows_%26_Apps).

## 4.7

### Changed
- Updated to GizmoSDK 2.12.326.1

## 4.6

### Added
- support for foliage on spherical maps.

### Fixed
- Camera controlls now support spherical maps.
- crash issue on certain maps (misaligned buffers)

### Changed
- Updated to GizmoSDK 2.12.309.1
- lowered foliage GPU memory bandwidth to increase performance.

## 4.5

### Added
- Support for uniform normal, single normal shared by geometry

### Fixed
- BFV and Map tool crashes when try to load an ETC2 map #363510

## 4.4

### Added
- Support for setting Unity layer for Node Builders 
- Support for setting Unity layer for Foliage 
- New WorldToUnity methods to MapUtil

## 4.3

### Added
- Support for SWEREF99 #341154

### Changed

- Updated to GizmoSDK 2.12.283.1
- Geometry no longer requires a texture
- Geometry no longer requires UV coordinates

### Fixed
- fixed normal tiling and weird glare (with maps without feature data)
- will now use 32 bit index buffers when required

## 4.2

### Changed

- Improved performance by recycling textures
- Updated to GizmoSDK 2.12.230.1

### Fixed

- Tile border visual glitch due to wrong texture wrap mode
- Fix restore NuGets in local scripts
- fixed Sun Glare
- fixed texture Tiling

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
- ETC2 maps crash

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