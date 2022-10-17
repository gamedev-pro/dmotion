using System;
using UnityEngine;

namespace DMotion.Authoring
{
    public class EnumTypeFilterAttribute : Attribute
    {
        
    }
    
    [Serializable]
    public struct SerializableType
    {
        public string AssemblyQualifiedTypeName;
        private Type cachedType;

        public Type Type
        {
            get => IsTypeValidInternal() ? cachedType : cachedType = GetTypeInternal();
            set
            {
                AssemblyQualifiedTypeName = value != null ? value.AssemblyQualifiedName : "";
                cachedType = null;
            }
        }

        private bool IsTypeValidInternal()
        {
            return cachedType != null && !string.IsNullOrEmpty(cachedType.AssemblyQualifiedName) &&
                   cachedType.AssemblyQualifiedName.Equals(AssemblyQualifiedTypeName);
        }

        private Type GetTypeInternal()
        {
            return string.IsNullOrEmpty(AssemblyQualifiedTypeName) ? null : Type.GetType(AssemblyQualifiedTypeName);
        }
    }
}