using System;
using System.Collections.Generic;
using Latios;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Entities;

namespace DMotion.Samples
{
    [UnityEngine.Scripting.Preserve]
    public class LatiosConversionBootstrap : ICustomConversionBootstrap
    {
        public bool InitializeConversion(World conversionWorldWithGroupsAndMappingSystems, CustomConversionSettings settings, ref List<Type> filteredSystems)
        {
            var defaultGroup = conversionWorldWithGroupsAndMappingSystems.GetExistingSystem<GameObjectConversionGroup>();
            BootstrapTools.InjectSystems(filteredSystems, conversionWorldWithGroupsAndMappingSystems, defaultGroup);
            KinemationConversionBootstrap.InstallKinemationConversion(conversionWorldWithGroupsAndMappingSystems);
            return true;
        }
    }

    #if !NETCODE_PROJECT
    [UnityEngine.Scripting.Preserve]
    public class LatiosBootstrap : ICustomBootstrap
    {
        public unsafe bool Initialize(string defaultWorldName)
        {
            var world                             = new LatiosWorld(defaultWorldName);
            World.DefaultGameObjectInjectionWorld = world;

            var systems = new List<Type>(DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default));
            BootstrapTools.InjectSystems(systems, world, world.simulationSystemGroup);

            CoreBootstrap.InstallImprovedTransforms(world);
            KinemationBootstrap.InstallKinemation(world);

            world.initializationSystemGroup.SortSystems();
            world.simulationSystemGroup.SortSystems();
            world.presentationSystemGroup.SortSystems();

            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            return true;
        }
    }
    #endif
}

