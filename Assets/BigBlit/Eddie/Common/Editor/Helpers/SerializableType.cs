using System;
using UnityEngine;

namespace BigBlit.Eddie
{
    [System.Serializable]
    internal class SerializableType
    {
        public string AssemblyQualifiedName => m_AssemblyQualifiedName;

        public Type Type => m_Type ??= System.Type.GetType(m_AssemblyQualifiedName);

        [SerializeField]
        private string m_AssemblyQualifiedName;

        private Type m_Type;


        public static bool IsValid(SerializableType serializableType)
        {
            return !(serializableType == null || string.IsNullOrEmpty(serializableType.m_AssemblyQualifiedName) || serializableType.Type == null);
        }


        public SerializableType(Type type)
        {
            m_Type = type;
            m_AssemblyQualifiedName = type.AssemblyQualifiedName;
        }

        public SerializableType(object obj)
        {
            m_Type = obj.GetType();
            m_AssemblyQualifiedName = m_Type.AssemblyQualifiedName;
        }

        public override bool Equals(System.Object obj)
        {
            SerializableType serializableType = obj as SerializableType;
            if ((object)serializableType == null)
                return false;

            return this.Equals(serializableType);
        }

        public bool Equals(SerializableType serializableType) => serializableType.Type.Equals(Type);

       public override int GetHashCode()
        {
            return m_AssemblyQualifiedName != null ? m_AssemblyQualifiedName.GetHashCode() : 0;
        }

        public static bool operator ==(SerializableType a, SerializableType b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(SerializableType a, SerializableType b) => !(a == b);

        public static implicit operator SerializableType(Type type) => new SerializableType(type);
        
        public static implicit operator Type(SerializableType seralizableType) => seralizableType.Type;
    }
}