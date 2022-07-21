using Latios.Authoring.Systems;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DOTSAnimation.Authoring
{
    internal struct ClipEventsConversionData
    {
        internal UnsafeList<DOTSAnimation.AnimationClipEvent> Events;
    }
    internal struct ClipEventsBlobConverter : ISmartBlobberSimpleBuilder<ClipEventsBlob>
    {
        internal UnsafeList<ClipEventsConversionData> ClipEvents;
        public unsafe BlobAssetReference<ClipEventsBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ClipEventsBlob>();
            var clipEvents = builder.Allocate(ref root.ClipEvents, ClipEvents.Length);
            for (var i = 0; i < clipEvents.Length; i++)
            {
                builder.ConstructFromNativeArray(ref clipEvents[i].Events, ClipEvents[i].Events.Ptr,
                    ClipEvents[i].Events.Length);
            }
            return builder.CreateBlobAssetReference<ClipEventsBlob>(Allocator.Persistent);
        }
    }
}