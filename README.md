# DMotion - A High Level Animation Framework and State Machine for DOTS

![image](https://user-images.githubusercontent.com/15620434/181682232-5e1eec98-d521-4b24-88be-ce447283bb12.png)

DMotion (DOTS Motion) is an opnionated general purpose Animation framework and State Machine for DOTS, built on top of [Kinemation](https://github.com/Dreaming381/Latios-Framework/tree/master/Kinemation).

I've built this tool with usability and performance in mind. *The runtime is 100% bursted, and it's currently ~6 times faster than Unity's Mechanim, for 10000 animated skeletons on screen at the same time.*

![image](https://user-images.githubusercontent.com/15620434/181847356-0f04a5e1-c5d4-4f6d-99c5-ecee51a379bb.png)

Overall, this system should feel similar to Unity's Mechanim system, however there is no intention to build a feature by feature clone of Mechanim, and I deliberately excluded features of Mechanim that I believe are not useful (more on that below), and added new use features that I've found are useful for games, which Mechanim does not support.

### Current Features (v0.3.0)

- Fully bursted runtime
- State Machine visual editor (and other custom editors)
- Play single clip
- 1D Blend Tree
- Loop and speed control
- Blending between states during transitions
- Boolean transitions
- End Time transtions
- Blend Parameters (float parameters for controlling Blend Trees)
- Animation Events
- Root Motion (with WriteGroup support, if you need to override default behaviour)
- Object Attachment
- One Shot animations (playing a single animation that is not present on the state machine)
- Support for optimized and non-optimized skeletons

### Coming soon

- Enum Transitions
- 2D Blend Tree (cartesian/freeform)
- State Machine Override (a.k.a Animator Override Controller)
- IK Support

### Planned
- Retargeting for Humanoid skeletons
- Multiple layers
- Skeleton Masks

### Features from Mechanim that I don't plan to implement

*Float and Int parameters for transitions*

I personally never found a reasonable use case for float and int based transitions. Float transitions are better replaced by Blend Trees (which DMotion supports), and I'm clueless to the purpose of int transitions (I've heard of developers using them for switching states based on equipped weapon, but AnimatorOverrides are a much cleaner solution).

If you have a use case for these features, let me know.

*StateMachineBehaviour (a.k.a some 'code' that runs when a state is active)*

I believe gameplay state should control animation state, not the way around. There is no plan to support anything similar to StateMachineBehaviour in DMotion

## Instalation

DOTSAnimation depend on [Kinemation](https://github.com/Dreaming381/Latios-Framework/tree/master/Kinemation), from Latios-Framework. You'll have to install both packages for it to work.

### Via `manifest.json`

Add the following lines to your `Packages/manifest.json` file

```json
"com.latios.latiosframework": "https://github.com/Dreaming381/Latios-Framework.git",
"com.gamedevpro.dmotion": "https://github.com/gamedev-pro/dmotion.git",
```

### Via Package Manager

- Window -> Package Manager -> Click the + button -> Add package from git url
- Paste `https://github.com/Dreaming381/Latios-Framework.git`
- Click again to add another git package
- Paste `https://github.com/gamedev-pro/dmotion.git`

### Via git submodule

- `cd` to your Packages folder
- `git submodule add https://github.com/Dreaming381/Latios-Framework.git`
- `git submodule add https://github.com/gamedev-pro/dmotion.git`

## Getting Started

I still have to write a proper documentation for this tool, for now hopefully the provided samples will help you get started.

- Make sure you have one of the Scriptable Rendering pipelines installed and setup (either URP or HDRP)
- After installing the package, go to `Window -> Package Manager` and click the DOTSAnimation package
- Expand the `Samples` dropdown and import the `All Samples`. This folder contains all examples for the package.
- The `1 - Complete State Machine` sample showcases every feature of the package at it's current state.
  - The State Machine setup is under `Data/StateMachine`, and it's hooked up to the player object via `AnimationStateMachineAuthoring` component.
- The `2 - Stress Test` sample can be used to check scalability of a simple state machine. Take a look at `StressTestSpawner` object in the sample scene. **Make sure to read the instructions on the scene ui for maximum performance**.
- The `3 - Animator Comparison` contains a comparison test between DMotion and Unity's Mechanim. It's similar to the StressTest scene.

## Support and Contributing

I will be supporting DMotion moving forward. Bug reports are definitely appreciated. If you can submit a PR with a bug fix, even better :).

Suggestions and feature requests will be considered. If you have something in mind, don't hesitate in letting me know.

