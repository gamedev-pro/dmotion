# DMotion - A High Level Animation Framework and State Machine for DOTS

![image](https://user-images.githubusercontent.com/15620434/181682232-5e1eec98-d521-4b24-88be-ce447283bb12.png)

DMotion (DOTS Motion) is a general purpose Animation framework and State Machine for DOTS, built on top of [Kinemation](https://github.com/Dreaming381/Latios-Framework/tree/master/Kinemation).

***Important Notes***:
  - *DMotion currently only supports Entities 0.51. Support for 1.0 will come in the future but no current ETA.*
  - *Kinemation only supports Windows, Linux and Mac. Mobile support is possible but require manual compilation of the ACL library, I suggest contacting the author of Kinemation for further information.*

I've built this tool with usability and performance in mind. *The runtime is 100% bursted, and it's currently ~6 times faster than Unity's Mechanim, for 10000 animated skeletons on screen at the same time.*

![image](https://user-images.githubusercontent.com/15620434/181847356-0f04a5e1-c5d4-4f6d-99c5-ecee51a379bb.png)

### Current Features (v0.3.4)

- Fully bursted runtime
- State Machine visual editor
- Transitions: Boolean, Int, Enum and End Time
- Simple API for playing clips through code (see samples)
- 1D Blend Tree
- Animation Events
- Root Motion (with WriteGroup support, if you need to override default behaviour)
- Object Attachment
- Support for optimized and non-optimized skeletons
- State Machine Visual Debugging

### Planned Features

- 2D Blend Tree (cartesian/freeform)
- State Machine Override (a.k.a Animator Override Controller)
- SubStates
- IK Support
- Multiple layers
- Skeleton Masks

## Instalation

### Requirements

DMotion requires [URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/InstallURPIntoAProject.html) or [HDRP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.1/manual/Upgrading-To-HDRP.html), and [ShaderGraph](https://docs.unity3d.com/Packages/com.unity.shadergraph@14.0/manual/Getting-Started.html) to be installed. 

### Install via Package Manager

- Window -> Package Manager -> Click the + button -> Add package from git url
- Paste `https://github.com/Dreaming381/Latios-Framework.git`
- Click again to add another git package
- Paste `https://github.com/gamedev-pro/dmotion.git`

### Install via git submodule

- `cd` to your Packages folder
- `git submodule add https://github.com/Dreaming381/Latios-Framework.git`
- `git submodule add https://github.com/gamedev-pro/dmotion.git`

## Getting Started

DMotion contains several samples and documentation to get you started. You can check out the [documentation here](https://github.com/gamedev-pro/dmotion/wiki/1.1-Getting-Started:-The-Basics).

## Support and Contributing

I will be supporting DMotion moving forward. Bug reports are definitely appreciated. If you can submit a PR with a bug fix, even better :).

Suggestions and feature requests will be considered. If you have something in mind about how this tool could be better for you, don't hesitate in letting me know.

