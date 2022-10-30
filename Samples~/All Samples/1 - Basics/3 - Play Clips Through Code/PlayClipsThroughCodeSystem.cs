using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayClipsThroughCode
{
    public struct PlayClipsThroughCodeComponent : IComponentData
    {
        public SingleClipRef WalkClip;
        public SingleClipRef RunClip;
        public float TransitionDuration;
    }

    [DisableAutoCreation]
    public partial class PlayClipsThroughCodeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var playWalk = Input.GetKeyDown(KeyCode.Alpha1);
            var playRun = Input.GetKeyDown(KeyCode.Alpha2);

            Entities.ForEach((ref PlaySingleClipRequest playSingleClipRequest,
                in PlayClipsThroughCodeComponent playClipsComponent) =>
            {
                if (playWalk)
                {
                    playSingleClipRequest = PlaySingleClipRequest.New(playClipsComponent.WalkClip,
                        loop: true,
                        playClipsComponent.TransitionDuration);
                }
                else if (playRun)
                {
                    playSingleClipRequest = PlaySingleClipRequest.New(playClipsComponent.RunClip,
                        loop: true,
                        playClipsComponent.TransitionDuration);
                }
            }).Schedule();
        }
    }
}