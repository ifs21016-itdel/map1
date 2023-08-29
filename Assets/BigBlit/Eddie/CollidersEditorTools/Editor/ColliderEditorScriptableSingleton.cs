using System;
using UnityEditor;


namespace BigBlit.Eddie.CollidersEditorTools
{
    internal abstract class ColliderEditorScriptableSingleton<T> : ScriptableSingleton<T> where T : ColliderEditorScriptableSingleton<T>
    {
        protected delegate void ForEachColliderFunction(ICollider collider);


        public virtual string Name => GetType().Name;
        public bool IsActive => m_activationCounter > 0;


        private int m_activationCounter = 0;


        public void Activate()
        {
            m_activationCounter++;
            if (m_activationCounter > 1)
                return;

            activate();
        }

        public void Deactivate()
        {
            if (m_activationCounter == 0)
                return;

            m_activationCounter--;
            if (m_activationCounter > 0)
                return;

            deactivate();
        }


        protected virtual void OnEnable()
        {
            ColliderTracker.trackedCollidersChanged += OnTrackedCollidersChanged;
            ColliderSelection.selectionChanged += OnSelectedCollidersChanged;

            if (m_activationCounter > 0)
                activate();
        }

        protected virtual void OnDisable()
        {
            if (m_activationCounter > 0)
                deactivate();

            ColliderTracker.trackedCollidersChanged -= OnTrackedCollidersChanged;
            ColliderSelection.selectionChanged -= OnSelectedCollidersChanged;
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }
        protected virtual void OnTrackedCollidersChanged() { }
        protected virtual void OnSelectedCollidersChanged() { }
        protected virtual void OnSceneGUI(SceneView sceneView) { }

        protected void ForEachCollider(ForEachColliderFunction forEachColliderFunction)
        {
            if (forEachColliderFunction == null)
                throw new ArgumentNullException("The argument 'forEachColliderFunction' cannot be null.");

            ICollider[] colliders = ColliderTracker.Colliders;
            foreach (var collider in colliders)
            {
                if (collider.IsTargetValid)
                    forEachColliderFunction(collider);
            }
        }

        protected void ForEachEnabledCollider(ForEachColliderFunction forEachColliderFunction)
        {
            if (forEachColliderFunction == null)
                throw new ArgumentNullException("The argument 'forEachColliderFunction' cannot be null.");

            ICollider[] colliders = ColliderTracker.EnabledColliders;
            foreach (var collider in colliders)
            {
                if (collider.IsTargetValid)
                    forEachColliderFunction(collider);
            }
        }
        protected void ForEachSelectedCollider(ForEachColliderFunction forEachColliderFunction)
        {
            if (forEachColliderFunction == null)
                throw new ArgumentNullException("The argument 'forEachColliderFunction' cannot be null.");

            ICollider[] colliders = ColliderSelection.Colliders;
            foreach (var collider in colliders)
            {
                if (collider.IsTargetValid)
                    forEachColliderFunction(collider);
            }
        }
        

        private void activate()
        {
            OnActivated();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void deactivate()
        {
            m_activationCounter = 0;
            SceneView.duringSceneGui -= OnSceneGUI;
            OnDeactivated();
        }


    }
}
