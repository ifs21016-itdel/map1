using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    class ColliderSelectionSerializer : ScriptableObject, ISerializationCallbackReceiver
    {
        private static ColliderSelectionSerializer s_Instance;


        public Collider[] m_SerializedColliders;


        [InitializeOnLoadMethod]
        private static void initialize()
        {
            if (s_Instance == null)
                s_Instance = Resources.FindObjectsOfTypeAll<ColliderSelectionSerializer>().FirstOrDefault<ColliderSelectionSerializer>();

            if (s_Instance == null)
            {
                s_Instance = CreateInstance<ColliderSelectionSerializer>();
                s_Instance.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.DontSave;
            }
        }

        public void OnAfterDeserialize()
        {
            if (m_SerializedColliders == null || m_SerializedColliders.Length == 0)
                return;

            var colliders = new List<ICollider>(m_SerializedColliders.Length);

            for (int i = 0; i < m_SerializedColliders.Length; i++)
            {
                var target = m_SerializedColliders[i];
                ICollider collider = null;
                Type type = target.GetType();
                if (type == typeof(BoxCollider))
                    collider = new BoxColliderProxy((BoxCollider)target);
                else if (type == typeof(SphereCollider))
                    collider = new SphereColliderProxy((SphereCollider)target);
                else if (type == typeof(CapsuleCollider))
                    collider = new CapsuleColliderProxy((CapsuleCollider)target);
                else
                    continue;

                colliders.Add(collider);
            }

            ColliderSelection.Colliders = colliders.ToArray();
        }

        public void OnBeforeSerialize() => m_SerializedColliders = ColliderSelection.RawColliders;
    }
}
