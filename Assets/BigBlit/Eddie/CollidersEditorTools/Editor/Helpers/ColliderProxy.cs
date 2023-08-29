using System;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal abstract class ColliderProxy : ICollider
    {
        protected Collider m_Target;

        public Collider Target => m_Target;
        public bool IsTargetValid => m_Target != null;
        public Type TargetType => m_Target.GetType();
        public Transform Transform => m_Target.transform;
        public GameObject GameObject => m_Target.gameObject;

        public abstract Vector3 Center { get; set; }
        public abstract Vector3 Size { get; set; }
        public abstract Vector3 WorldCenter { get; set; }
        public abstract Bounds WorldBounds { get; }
        public abstract Vector3 HandleCenter { get; set; }
        public abstract Vector3 HandleSize { get; set; }
        public abstract Bounds Bounds { get; set; }
        public abstract Bounds HandleBounds { get; set; }

        public ColliderProxy(Collider collider)
        {
            m_Target = collider;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ColliderProxy proxy && proxy.m_Target == m_Target;
        }

        public override int GetHashCode() => m_Target != null ? m_Target.GetHashCode() : 0;
        public abstract void SetHandleMinMax(Vector3 min, Vector3 max);
        public abstract void Rotate(Vector3 angles);
    }

}
