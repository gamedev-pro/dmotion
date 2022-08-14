#if NETCODE_PROJECT
using System;
using Latios;
using Latios.Compatibility.UnityNetCode;
using Unity.Entities;

namespace DMotion.Samples
{
    [UnityEngine.Scripting.Preserve]
    public class NetCodeLatiosBootstrap : LatiosClientServerBootstrapBase
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979;  // Enable auto connect
            return base.Initialize(defaultWorldName);
        }

        public override World CreateCustomClientWorld(string worldName)
        {
            var world = new LatiosWorld(worldName, WorldFlags.Game, LatiosWorld.WorldRole.Client);

            BootstrapTools.InjectSystems(ClientSystems, world, world.simulationSystemGroup, ClientGroupRemap);

            CoreBootstrap.InstallImprovedTransforms(world);
            Latios.Kinemation.KinemationBootstrap.InstallKinemation(world);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();
            world.presentationSystemGroup.SortSystems();

            return world;
        }

        public override World CreateCustomServerWorld(string worldName)
        {
            var world = new LatiosWorld(worldName, WorldFlags.Game, LatiosWorld.WorldRole.Server);

            BootstrapTools.InjectSystems(ServerSystems, world, world.simulationSystemGroup, ServerGroupRemap);

            CoreBootstrap.InstallImprovedTransforms(world);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();

            return world;
        }
    }
}
#endif
