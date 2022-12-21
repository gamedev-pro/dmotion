using System.Reflection;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    public abstract class ECSTestBase : ECSTestsFixture
    {
        private const float defaultDeltaTime = 1.0f / 60.0f;
        private float elapsedTime;

        public override void Setup()
        {
            base.Setup();
            elapsedTime = Time.time;
            //Create required systems
            {
                var requiredSystemsAttr = GetType().GetCustomAttribute<CreateSystemsForTest>();
                if (requiredSystemsAttr != null)
                {
                    var baseType = typeof(SystemBase);
                    foreach (var t in requiredSystemsAttr.SystemTypes)
                    {
                        Assert.IsTrue(baseType.IsAssignableFrom(t),
                            $"Expected {t.Name} to be a subclass of {baseType.Name}");
                        world.CreateSystem(t);
                    }
                }
            }
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
                    manager.CompleteAllTrackedJobs();
                }
            }
        }
    }
}