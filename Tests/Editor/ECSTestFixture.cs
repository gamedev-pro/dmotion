using NUnit.Framework;
using System.Reflection;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
#if !UNITY_DOTSRUNTIME
using UnityEngine.LowLevel;
#endif

#if NET_DOTS
    public class EmptySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }

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

        public unsafe new BufferFromEntity<T> GetBufferFromEntity<T>(bool isReadOnly = false) where T : struct, IBufferElementData
        {
            CheckedState()->AddReaderWriter(isReadOnly ? ComponentType.ReadOnly<T>() : ComponentType.ReadWrite<T>());
            return EntityManager.GetBufferFromEntity<T>(isReadOnly);
        }
    }
#else
public partial class EmptySystem : SystemBase
{
	protected override void OnUpdate() {}

	public new EntityQuery GetEntityQuery(params EntityQueryDesc[] queriesDesc) => base.GetEntityQuery(queriesDesc);

	public new EntityQuery GetEntityQuery(params ComponentType[] componentTypes) => base.GetEntityQuery(componentTypes);

	public new EntityQuery GetEntityQuery(NativeArray<ComponentType> componentTypes) => base.GetEntityQuery(componentTypes);
}

#endif

public class ECSTestsCommonBase : ScriptableObject
{
	[SetUp]
	public virtual void Setup()
	{
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.EnterScope();
#endif
	}

	[TearDown]
	public virtual void TearDown()
	{
#if UNITY_DOTSRUNTIME
            Unity.Runtime.TempMemoryScope.ExitScope();
#endif
	}
}

/// <summary>
/// Copied from the Entities package and slightly modified to enable default world creation and fixing a call to an internal method via reflection.
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

	protected int stressTestEntityCount = 1000;
	protected bool createDefaultWorld = false;
	private bool jobsDebuggerWasEnabled;

	private float elapsedTime;
	private const float defaultDeltaTime = 1.0f / 60.0f;

	[SetUp]
	public override void Setup()
	{
		base.Setup();

#if !UNITY_DOTSRUNTIME
		// unit tests preserve the current player loop to restore later, and start from a blank slate.
		previousPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
#endif

		previousWorld = world;
		world = World.DefaultGameObjectInjectionWorld =
			createDefaultWorld ? DefaultWorldInitialization.Initialize("Default Test World") : new World("Empty Test World");
		manager = world.EntityManager;
		managerDebug = new EntityManager.EntityManagerDebug(manager);

		// Many ECS tests will only pass if the Jobs Debugger enabled;
		// force it enabled for all tests, and restore the original value at teardown.
		jobsDebuggerWasEnabled = JobsUtility.JobDebuggerEnabled;
		JobsUtility.JobDebuggerEnabled = true;
#if !UNITY_DOTSRUNTIME
		JobUtility_ClearSystemIds();
#endif
		elapsedTime = Time.time;
	}

	[TearDown]
	public override void TearDown()
	{
		if (world != null && world.IsCreated)
		{
			// Clean up systems before calling CheckInternalConsistency because we might have filters etc
			// holding on SharedComponentData making checks fail
			while (world.Systems.Count > 0)
				world.DestroySystem(world.Systems[0]);

			managerDebug.CheckInternalConsistency();

			world.Dispose();
			world = null;

			world = previousWorld;
			previousWorld = null;
			manager = default;
		}

		JobsUtility.JobDebuggerEnabled = jobsDebuggerWasEnabled;
#if !UNITY_DOTSRUNTIME
		//JobsUtility.ClearSystemIds();
		JobUtility_ClearSystemIds();
#endif

#if !UNITY_DOTSRUNTIME
		PlayerLoop.SetPlayerLoop(previousPlayerLoop);
#endif

		base.TearDown();
	}

	protected void UpdateWorld(float deltaTime = defaultDeltaTime, bool completeAllJobs = true)
	{
		if (world != null && world.IsCreated)
		{
			elapsedTime += deltaTime;
			world.SetTime(new TimeData(elapsedTime, deltaTime));
			foreach (var s in world.Systems)
			{
				s.Update();
			}

			if (completeAllJobs)
			{
				manager.CompleteAllJobs();
			}
		}
	}

	// calls JobUtility.ClearSystemIds() (internal method)
	private void JobUtility_ClearSystemIds() =>
		typeof(JobsUtility).GetMethod("ClearSystemIds", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
}
