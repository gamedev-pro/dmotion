using System;
using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CreateSystemsForTest : Attribute
    {
        public Type[] SystemTypes;

        public CreateSystemsForTest(params Type[] systemTypes)
        {
            SystemTypes = systemTypes;
        }
    }
}