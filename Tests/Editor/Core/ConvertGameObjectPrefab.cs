using System;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConvertGameObjectPrefab : Attribute
    {
        public string ToFieldName;
        public ConvertGameObjectPrefab(string toFieldName)
        {
            ToFieldName = toFieldName;
        }
    }
}