using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    public struct SingleClipState : IBufferElementData
    {
        public byte AnimationStateId;
    }
}