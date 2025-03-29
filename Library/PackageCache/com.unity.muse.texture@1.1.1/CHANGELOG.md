# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2024-08-07

## [1.1.0] - 2024-08-06

## [1.1.0-pre.6] - 2024-08-02

## [1.1.0-pre.5] - 2024-07-31

### Changed
- Rename "Muse Texture Tool" to "Muse Texture".

## [1.0.0] - 2024-04-15

### Changed

- Update version to "1.0.0".

## [1.0.0-pre.21] - 2024-04-05

### Changed

- Update com.unity.muse.common dependency version.

## [1.0.0-pre.20] - 2024-04-02

## [1.0.0-pre.19] - 2024-03-14

## [1.0.0-pre.18] - 2024-03-11

## [1.0.0-pre.5] - 2024-02-16

### Changed

- Update com.unity.muse.common dependency version.

## [1.0.0-pre.4] - 2024-02-16

### Changed

- Prompt fields are now pre-populated when going into refine mode.
- Prompt fields are now disabled when using in-painting.

### Fixed

- Fix Generations label overflowing.
- Fix Material preview rotation that was resetting.

## [1.0.0-pre.3] - 2023-12-15

### Fixed

- Fix error when trying to build a Unity Project.

## [1.0.0-pre.2] - 2023-11-16

### Fixed

- Fix ellipsis not visible in refine mode.

## [1.0.0-pre.1] - 2023-11-16

### Added

- Add ColorPicker.
- Add feedback system to textures.

### Changed

- Change PBR preview visuals.
- Rename diffuseMap to albedoMap.
- Change exported material name to human readable name.
- Color input images cannot be greater than 512x512 pixels.

### Fixed

- Fix strength for Input Image not working properly.

### Removed

- Remove Samples.

## [0.4.1] - 2023-10-20

### Added

- Add option for Default Path of Muse assets in Preferences.

### Changed

- Change minimum version of shadergraph to "14.0.7".
- Reduce import time of HDRI images.
- Change Roughness references to Smoothness.

### Fixed

- Retain textures on scene changed.
- Fix default camera being replaced.
- Fix preview scenes leaking.
- Fix NullReferenceException when using the inpainting tool.
- Fix empty canvas when a texture is loaded.
- Fix UltraLiteDB could not be found when doing builds.

## [0.3.1] - 2023-10-03

### Fixed

- Fix camera settings compatibility issues with the HDRP package.

## [0.3.0] - 2023-09-28

### Added

- Add an option to toggle the vertex displacement in PBR refinement.
- Add Input Image operator.

### Changed

- Change Cylinder and Cube in PBR refinement.
- No need to shift + click to rotate the preview in PBR refinement.

## [0.2.0] - 2023-09-20

### Added

- Add error message and retry button when generation has failed.

### Changed

- PBR options are always visible in refinement.
- Change PBR UI.
- Disable in-painting during PBR refinement.

### Fixed

- Fix invalid artifacts not loading.
- Fix inverted normal map on export.
- Fix artifact preview not loading when creating a new scene.
- Fix no default selected preview item in refinement.

## [0.1.2] - 2023-09-12

## [0.1.1] - 2023-08-28

## [0.1.0] - 2023-06-10

### Added

- Initial release of the Unity Muse AI Tools package.
