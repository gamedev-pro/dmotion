using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayOneShot
{
    [DisableAutoCreation]
    public partial class PlayOneShotExampleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var shouldPlayOneShot = Input.GetKeyDown(KeyCode.Space);
            Entities.ForEach((ref PlayOneShotRequest playOneShotRequest,
                in PlayOneShotExampleComponent playOneShotExampleComponent) =>
            {
                if (shouldPlayOneShot)
                {
                    playOneShotRequest = PlayOneShotRequest.New(playOneShotExampleComponent.OneShotClipRef,
                        playOneShotExampleComponent.TransitionDuration, playOneShotExampleComponent.NormalizedEndTime);
                }
            }).Schedule();
        }
    }
}