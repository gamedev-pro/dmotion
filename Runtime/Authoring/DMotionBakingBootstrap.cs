using Latios.Authoring;

namespace DMotion.Authoring
{
    public static class DMotionBakingBootstrap
    {
        /// <summary>
        /// Adds Kinemation bakers and baking systems into baking world and disables the Entities.Graphics's SkinnedMeshRenderer bakers
        /// </summary>
        /// <param name="world">The conversion world in which to install the Kinemation conversion systems</param>
        public static void InstallDMotionBakersAndSystems(ref CustomBakingBootstrapContext context)
        {
            context.systemTypesToInject.Add(typeof(AnimationStateMachineSmartBlobberSystem));
            context.systemTypesToInject.Add(typeof(ClipEventsSmartBlobberSystem));
        }
    }
}