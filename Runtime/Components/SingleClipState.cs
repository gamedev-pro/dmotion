using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct SingleClipState : IBufferElementData
    {
        internal byte PlayableId;

    }
}