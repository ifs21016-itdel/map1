using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal abstract class ColliderTool : ScriptableObject
    {
        private ICollider[] m_Targets;


        public ICollider[] Targets
        {
            get => m_Targets;
            set => m_Targets = (value == null) ? new ICollider[] { } : value;
        }

        public virtual string Name => GetType().Name;

        public virtual void OnActivated() { }

        public virtual void OnDeactivated() { }

        public virtual void OnTargetsSet() { }
    }
}
