# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic
Versioning](http://semver.org/spec/v2.0.0.html).

## [0.5.3-preview-1] – 2022-7-21

This marks the first version ready for public use and with somewhat stable API.

### Added

- State Machines can now be built with Scriptable Objects
- Custom Animation clip and evens editor with preview objects
- Root Motion Modes
    - Disabled: No root motion
    - Enabled Automatic: Root motion is automatically applied to the root object
    - Enabled Manual: Root motion deltas are calculated and stored to be used by external systems for custom use cases
- Enable/Disable animation events

### Changed

- Moved most StateMachine data to Blob Assets
- AnimationEvents implementation now use DynamicBuffers
- API cleanup pass (implementation details made internal)

## [0.0.1] – 2022-6-25

Very first version for community feedback

### Added

- Animation State Machine System and Authoring functions
- Support for optimized and non-optimized bones
- Single Clip States and 1D Blend Tree States
- Blending between states during transitions
- Boolean based transitions between states (OR/AND)
- End time transitions between states
- Root Motion (with WriteGroup support)
- Animation Events
- One Shot animations (playing a single animation that is not on the state machine)
- Object attachment
