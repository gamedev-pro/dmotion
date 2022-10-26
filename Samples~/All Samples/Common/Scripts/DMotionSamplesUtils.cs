using Unity.Entities;

namespace DMotion.Samples.Common
{
    public static class DMotionSamplesUtils
    {
        public static void AddSytemToPlayerUpdate<T>(EntityManager entityManager)
            where T : SystemBase, new()
        {
            var s = entityManager.World.GetOrCreateSystem<T>();
            var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            ScriptBehaviourUpdateOrder.AppendSystemToPlayerLoop(s, ref playerLoop, typeof(UnityEngine.PlayerLoop.Update));
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
        }
    }
}