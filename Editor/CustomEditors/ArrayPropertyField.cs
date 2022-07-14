using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    public class ArrayPropertyField : PropertyField
    {
        public Action ArrayChanged;
        private SerializedProperty property;
        private int prevArraySize;

        public ArrayPropertyField(SerializedProperty prop) : base(prop)
        {
            property = prop;
            Assert.IsTrue(property.isArray);
            prevArraySize = property.arraySize;
        }
        
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt is GeometryChangedEvent && property.arraySize != prevArraySize)
            {
                prevArraySize = property.arraySize;
                ArrayChanged?.Invoke();
            }
        }
    }
}