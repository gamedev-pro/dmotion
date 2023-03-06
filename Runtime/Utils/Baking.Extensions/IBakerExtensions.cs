using Unity.Entities;

namespace DMotion.Authoring
{
    public static class IBakerExtensions
    {
        public static EntityCommandBuffer GetEcb(this IBaker baker)
        {
            return baker._State.Ecb;
        }
    }
}