using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayClipsThroughCode
{
    [RequireMatchingQueriesForUpdate]
    public partial struct PlayClipsThroughCodeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var playWalk = Input.GetKeyDown(KeyCode.Alpha1);
            var playRun = Input.GetKeyDown(KeyCode.Alpha2);

            foreach (var (playSingleClipRequest, playClipsComponent) in
                     SystemAPI.Query<RefRW<PlaySingleClipRequest>, PlayClipsThroughCodeComponent>())
            {
                if (playWalk)
                {
                    playSingleClipRequest.ValueRW = PlaySingleClipRequest.New(playClipsComponent.WalkClip,
                        loop: true,
                        playClipsComponent.TransitionDuration);
                }
                else if (playRun)
                {
                    playSingleClipRequest.ValueRW = PlaySingleClipRequest.New(playClipsComponent.RunClip,
                        loop: true,
                        playClipsComponent.TransitionDuration);
                }
            }
        }
    }
}