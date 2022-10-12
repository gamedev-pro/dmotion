using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class StateMachineTestUtils
    {
        public static Entity InstantiateStateMachineEntity(this EntityManager manager, Entity prefab)
        {
            var newEntity = manager.Instantiate(prefab);
            Assert.IsTrue(manager.HasComponent<AnimationStateMachine>(newEntity));
            return newEntity;
        }

        public static void SetBoolParameter(this EntityManager manager, Entity entity, int index, bool newValue)
        {
            Assert.IsTrue(manager.HasComponent<BoolParameter>(entity));
            var boolParameters = manager.GetBuffer<BoolParameter>(entity);
            Assert.IsTrue(boolParameters.Length > 0);
            var parameter = boolParameters[index];
            parameter.Value = newValue;
            boolParameters[index] = parameter;
        }
        
        public static void SetIntParameter(this EntityManager manager, Entity entity, int index, int newValue)
        {
            Assert.IsTrue(manager.HasComponent<IntParameter>(entity));
            var intParameters = manager.GetBuffer<IntParameter>(entity);
            Assert.IsTrue(intParameters.Length > 0);
            var parameter = intParameters[index];
            parameter.Value = newValue;
            intParameters[index] = parameter;
        }
    }
}