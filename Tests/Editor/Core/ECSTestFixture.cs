using System.Reflection;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
#if !UNITY_DOTSRUNTIME
using UnityEngine.LowLevel;
#endif

/*
 * IMPORTANT: This file is copied directly from Unity.Entities package. Edit ECSTestBase instead
 */
namespace DMotion.Tests
{
    // If ENABLE_UNITY_COLLECTIONS_CHECKS is not defined we will ignore the test
    // When using this attribute, consider it to logically AND with any other TestRequiresxxxx attrubute
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    internal class TestRequiresCollectionChecks : System.Attribute
    {
        public TestRequiresCollectionChecks(string msg = null) { }
    }
#else
    internal class TestRequiresCollectionChecks : IgnoreAttribute
    {
        public TestRequiresCollectionChecks(string msg = null) : base($"Test requires ENABLE_UNITY_COLLECTION_CHECKS which is not defined{(msg == null ? "." : $": {msg}")}") { }
    }
#endif

    // If ENABLE_UNITY_COLLECTIONS_CHECKS and UNITY_DOTS_DEBUG is not defined we will ignore the test
    // conversely if either of them are defined the test will be run.
    // When using this attribute, consider it to logically AND with any other TestRequiresxxxx attrubute
#if ENABLE_UNITY_COLLECTIONS_CHECKS || UNITY_DOTS_DEBUG
    internal class TestRequiresDotsDebugOrCollectionChecks: System.Attribute
    {
        public TestRequiresDotsDebugOrCollectionChecks(string msg = null) { }
    }
#else
    internal class TestRequiresDotsDebugOrCollectionChecks : IgnoreAttribute
    {
        public TestRequiresDotsDebugOrCollectionChecks(string msg = null) : base($"Test requires UNITY_DOTS_DEBUG || ENABLE_UNITY_COLLECTION_CHECKS which neither are defined{(msg == null ? "." : $": {msg}")}") { }
    }
#endif

    // Ignores te test when in an il2cpp build only. Please make use of the 'msg' string
    // to tell others why this test should be ignored
#if !ENABLE_IL2CPP
    internal class IgnoreTest_IL2CPP: System.Attribute
    {
        public IgnoreTest_IL2CPP(string msg = null) { }
    }
#else
    internal class IgnoreTest_IL2CPP : IgnoreAttribute
    {
        public IgnoreTest_IL2CPP(string msg = null) : base($"Test ignored on IL2CPP builds{(msg == null ? "." : $": {msg}")}") { }
    }
#endif

    public partial class EmptySystem : SystemBase
    {
        protected override void OnUpdate() {}

        public new EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc)
        {
            return base.GetEntityQuery(queriesDesc);
        }

        public new EntityQuery GetEntityQuery(params ComponentType[] componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }

        public new EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes)
        {
            return base.GetEntityQuery(componentTypes);
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public class ECSTestsCommonBase : ScriptableObject
    {
        [SetUp]
        public virtual void Setup()
        {
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.EnterScope();
            UnityEngine.TestTools.LogAssert.ExpectReset();
#endif
        }

        [TearDown]
        public virtual void TearDown()
        {
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.ExitScope();
#endif
        }

        [BurstDiscard]
        static public void TestBurstCompiled(ref bool falseIfNot)
        {
            falseIfNot = false;
        }

        [BurstCompile(CompileSynchronously = true)]
        static public bool IsBurstEnabled()
        {
            bool burstCompiled = true;
            TestBurstCompiled(ref burstCompiled);
            return burstCompiled;
        }

    }

    /// <summary>
    /// This is copied directly from Unity.Entities package
    /// </summary>
    public abstract class ECSTestsFixture : ECSTestsCommonBase
    {
        protected World previousWorld;
        protected World world;
#if !UNITY_DOTSRUNTIME
        protected PlayerLoopSystem previousPlayerLoop;
#endif
        protected EntityManager manager;
        protected EntityManager.EntityManagerDebug managerDebug;

        private bool JobsDebuggerWasEnabled;


        [SetUp]
        public override void Setup()
        {
            base.Setup();

#if !UNITY_DOTSRUNTIME
            // unit tests preserve the current player loop to restore later, and start from a blank slate.
            previousPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
#endif

            previousWorld = World.DefaultGameObjectInjectionWorld;
            world = World.DefaultGameObjectInjectionWorld = new World("Test World");
            world.UpdateAllocatorEnableBlockFree = true;
            manager = world.EntityManager;
            managerDebug = new EntityManager.EntityManagerDebug(manager);

            // Many ECS tests will only pass if the Jobs Debugger enabled;
            // force it enabled for all tests, and restore the original value at teardown.
            JobsDebuggerWasEnabled = JobsUtility.JobDebuggerEnabled;
            JobsUtility.JobDebuggerEnabled = true;

#if !UNITY_DOTSRUNTIME
            JobUtility_ClearSystemIds();
#endif

#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !DISABLE_ENTITIES_JOURNALING
            // In case entities journaling is initialized, clear it
            EntitiesJournaling.Clear();
#endif
        }
        
        [TearDown]
        public override void TearDown()
        {
            if (world != null && world.IsCreated)
            {
                // Clean up systems before calling CheckInternalConsistency because we might have filters etc
                // holding on SharedComponentData making checks fail
                while (world.Systems.Count > 0)
                {
                    world.DestroySystemManaged(world.Systems[0]);
                }

                managerDebug.CheckInternalConsistency();

                world.Dispose();
                world = null;

                World.DefaultGameObjectInjectionWorld = previousWorld;
                previousWorld = null;
                manager = default;
            }

            JobsUtility.JobDebuggerEnabled = JobsDebuggerWasEnabled;

#if !UNITY_DOTSRUNTIME
            JobUtility_ClearSystemIds();
#endif

#if !UNITY_DOTSRUNTIME
            PlayerLoop.SetPlayerLoop(previousPlayerLoop);
#endif

            base.TearDown();
        }
        
        partial class EntityForEachSystem : SystemBase
        {
            protected override void OnUpdate() {}
        }

        public EmptySystem EmptySystem
        {
            get
            {
                return World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EmptySystem>();
            }
        }
        
        
    // calls JobUtility.ClearSystemIds() (internal method)
    private void JobUtility_ClearSystemIds() =>
        typeof(JobsUtility).GetMethod("ClearSystemIds", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, null);
    }
}
