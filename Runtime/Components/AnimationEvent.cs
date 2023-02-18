using System;
using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    public struct RaisedAnimationEvent : IBufferElementData
    {
        //the animated entity that received the event
        public Entity Entity;
        public int EventHash;
    }
    
    internal struct AnimationClipEvent
    {
        internal int EventHash;
        internal float ClipTime;
    }

    public struct ClipEvents
    {
        internal BlobArray<AnimationClipEvent> Events;
    }
    
    public struct ClipEventsBlob
    {
        internal BlobArray<ClipEvents> ClipEvents;
    }
}