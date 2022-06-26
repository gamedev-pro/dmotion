using Unity.Collections;
using Unity.Entities;

namespace DOTSAnimation
{
    public struct AnimationStateRef
    {
        internal int Index;
    }
    
    public struct AnimationStateMachine : IComponentData
    {
        internal StateRef CurrentState;
        internal StateRef NextState;
        internal StateRef PrevState;
        internal StateRef RequestedNextState;
        internal struct StateRef
        {
            internal int StateIndex;
            internal bool IsOneShot;

            internal bool IsValid => StateIndex >= 0;

            internal static StateRef Null => new StateRef() { StateIndex = -1 };
        }

    }

    public struct BoolParameter : IBufferElementData
    {
        public int Hash;
        public bool Value;
    }
    
    public struct BlendParameter : IBufferElementData
    {
        public int Hash;
        public int StateIndex;
        public float Value;
    }
}