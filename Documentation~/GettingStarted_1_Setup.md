# Getting Started Pt 1: The Basics

### Before proceeding in this section make sure you have

- [URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/InstallURPIntoAProject.html) or [HDRP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@12.1/manual/Upgrading-To-HDRP.html) installed on your Unity project
- [ShaderGraph](https://docs.unity3d.com/Packages/com.unity.shadergraph@14.0/manual/Getting-Started.html) installed o your project
- [Installed DMotion](../README.md#Instalation)

## Importing and Running the Samples

Go to `Window -> Package Manager` select `Packages: In Project` in the dropdown, click the `DMotion` package, and import the samples

![image](https://user-images.githubusercontent.com/101072561/198904077-a61245fd-e8c7-4370-9944-10b4b756ed9a.png)

You'll see a new folder on `Samples/DMotion`. There are multiple samples there which we will go over in detail. For now, Open the scene named `DMotion - Basic - Play Single Clip` under `All Samples/1 - Basics/1 - Play Single Clip`. 

Hit play and see the cool robot walking in a loop :)

![image](https://user-images.githubusercontent.com/101072561/198904233-b65246b4-3f9f-4490-b002-6f4d09e93219.png)

Of course, you probably want to know what is happening in this scene. So let's explore the setup in the next section...

## Basics: Playing a Clip

### 1. Setting up your Model

In order for a Skeletal Mesh to work with DMotion, it needs some setup. Go to `All Samples/Common/Art/Models/LowPolyRobot` and select `LowPolyRobot.fbx`. You will notice it has the following import settings:

  - Model tab:
    - (Required): `Read/Write` enabled
  - Rig tab:
    - (Recommended) `Optimized Game Objects` enabled
    - (Recommended for Humanoid clips): `Animation Type` set to Humanoid
  - Animation Tab:
    - (Recommended) Set `Anim. Compression` to `Off`

![image](https://user-images.githubusercontent.com/101072561/198904776-9975cf33-0099-455e-b2e5-a0843c9d2fe5.png)

<details>
<summary>Why turn off Anim. Compression?</summary>
DMotion uses a custom compression algorithm provided by Kinemation. If it reads from Unity’s lossy-compressed clips, quality will be significantly reduced without any benefit.
</details>

After you've done a similar setup with your Model, it should be ready to be used with DMotion.

### 2. Latios Bootstrap

DMotion currently used Kinemation low level framewok for Animation sampling and skinning. This framework requires a Latios Bootstrap to be added to your project.

This is already set up for you under `All Samples/Common/Scripts/LatiosBootstrap.cs`, so unless you are using a custom bootstrap, you don't need to do anything else. Just make sure to move `LatiosBootstrap.cs` to another folder in your project **if** you sant to delete the Samples folders.

### 3. Creating a Clip Asset

DMotion uses a custom scriptable object for defining Animation clips, which has more features than Unity's AnimationClip. You can create a `AnimationClipAsset` by going to `Create -> DMotion -> Clip`

![image](https://user-images.githubusercontent.com/101072561/198905400-29722989-c645-40a6-a0dd-2120e37ac1d7.png)

If you select `1 - Play Single Clip/1.1_PlaySingleClip_Walk.asset`, you will see the AnimationClipAsset we're using in this sample. Below it's a brief explanation of the asset's properties:

| Property  | Definition |
| ------------- | ------------- |
| Preview Object  | Editor-only preview mesh. Used for previewing the selected clip  |
| Clip  | `AnimationClip` to be played  |
| Events  | List of `AnimationClipEvent` for this clip. See this page for more information on Animation Events.  |

![image](https://user-images.githubusercontent.com/101072561/198905356-e1858992-ae30-4b4e-ac57-ac522916676d.png)

### 4. PlayClipAuthoring component

Now, let's go back to the `DMotion - Basic - Play Single Clip` scene in Unity. If you select the `LowPolyRobot` game object in the hierarchy, you'll see the a component called `PlayClipAuthoring`

This component is provided by DMotion, and it's the only thing you need if all you want is o play a single clip in an object (of course, we will do much cooler things in the following sections). For now, here's a description of it's properties:

| Property  | Definition |
| ------------- | ------------- |
| Owner | Owner of the animated object. Useful when your logical GameObject and the GameObject being animated are different. Otherwise, it should be the same GameObject containing the Animator omponent  |
| Animator  | Object containing the `Animator` component  |
| Clip  | Clip to be played (with speed and loop options) |
| RootMotionMode  | Option for RootMotion handling. See this page for more information on automatic root motion |
| Enable Events  | Whether `AnimationClipEvents` should be enabled |
| Enable Single Clip Requests  | Whether to enable other clips to be requested via code. See this documentation page for more information. |

![image](https://user-images.githubusercontent.com/101072561/198905210-d97bc1e1-6544-4b88-b0e8-c8a9e4a849ef.png)

### 5. Conclusion

This section explained how to setup a model for DMotion, and how to play a clip without writing any code. In the next section you'll learn about Animation Events.





