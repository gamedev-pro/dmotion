using System;
using System.Collections.Generic;
using DMotion.Authoring;
using Latios;
using Latios.Authoring;
using Latios.Kinemation.Authoring;
using Unity.Entities;

namespace DMotion.Samples.Common
{
    [UnityEngine.Scripting.Preserve]
    public class LatiosBakingBootstrap : ICustomBakingBootstrap
    {
        public void InitializeBakingForAllWorlds(ref CustomBakingBootstrapContext context)
        {
            KinemationBakingBootstrap.InstallKinemationBakersAndSystems(ref context);
            DMotionBakingBootstrap.InstallDMotionBakersAndSystems(ref context);
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class LatiosEditorBootstrap : ICustomEditorBootstrap
    {
        public World InitializeOrModify(World defaultEditorWorld)
        {
            var world = new LatiosWorld(defaultEditorWorld.Name);

            var systems =
                new List<Type>(DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default, true));
            BootstrapTools.InjectSystems(systems, world, world.simulationSystemGroup);

            CoreBootstrap.InstallImprovedTransforms(world);
            Latios.Kinemation.KinemationBootstrap.InstallKinemation(world);

            return world;
        }
    }

    [UnityEngine.Scripting.Preserve]
    public class LatiosBootstrap : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            var world = new LatiosWorld(defaultWorldName);
            World.DefaultGameObjectInjectionWorld = world;

            var systems = new List<Type>(DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default));
            BootstrapTools.InjectSystems(systems, world, world.simulationSystemGroup);

            CoreBootstrap.InstallImprovedTransforms(world);
            Latios.Kinemation.KinemationBootstrap.InstallKinemation(world);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();
            world.presentationSystemGroup.SortSystems();

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            return true;
        }
    }
}