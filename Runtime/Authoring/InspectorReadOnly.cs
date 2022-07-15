using System;

namespace DOTSAnimation.Authoring
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class InspectorReadOnly : Attribute{}
}