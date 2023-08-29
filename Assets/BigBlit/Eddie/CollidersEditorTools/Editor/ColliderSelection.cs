using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    [InitializeOnLoad]
    internal static class ColliderSelection
    {
        public static event Action selectionChanged;

        public static ICollider Collider
        {
            get
            {
                if (m_IsDirty)
                    validateColliders();

                return s_Colliders.Count > 0 ? s_Colliders[0] : null;
            }

            set
            {
                if (value == null)
                {
                    Clear();
                }
                else
                {
                    if (s_Colliders.Count == 1 && s_Colliders[0] == value)
                        return;

                    s_Colliders.Clear();
                    s_Colliders.Add(value);
                    m_IsDirty = true;
                    selectionChanged?.Invoke();
                }
            }
        }

        public static ICollider[] Colliders
        {
            get
            {
                if (m_IsDirty)
                    validateColliders();

                return s_Colliders.ToArray();
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    Clear();
                }
                else
                {
                    s_Colliders.Clear();
                    s_Colliders.AddRange(value);
                    m_IsDirty = true;
                    selectionChanged?.Invoke();
                }
            }
        }

        public static void Add(ICollider collider)
        {
            if (s_Colliders.Contains(collider))
                return;

            s_Colliders.Insert(0, collider);
            m_IsDirty = true;
            selectionChanged?.Invoke();
        }

        public static void Remove(ICollider collider)
        {
            if (!s_Colliders.Contains(collider))
                return;

            if (s_Colliders.Count == 1)
            {
                Clear();
            }
            else
            {
                s_Colliders.Remove(collider);
                m_IsDirty = true;
                selectionChanged?.Invoke();
            }
        }

        public static void Clear()
        {
            if (s_Colliders.Count == 0)
                return;

            s_Colliders.Clear();
            m_CollidersArray = new ICollider[] { };
            selectionChanged?.Invoke();
        }

        public static void Add(IEnumerable<ICollider> colliders)
        {
            bool isDirty = false;
            foreach (var collider in colliders)
            {
                if (s_Colliders.Contains(collider))
                    continue;
                s_Colliders.Add(collider);
                isDirty = true;
            }

            if (!isDirty)
                return;

            m_IsDirty = true;
            selectionChanged?.Invoke();
        }

        public static void Remove(IEnumerable<ICollider> colliders)
        {
            bool isDirty = false;
            foreach (var collider in colliders)
            {
                if (!s_Colliders.Contains(collider))
                    continue;
                s_Colliders.Remove(collider);
                isDirty = true;
            }

            if (!isDirty)
                return;

            m_IsDirty = true;
            selectionChanged?.Invoke();
        }

        public static Collider RawCollider => Collider?.Target;
        public static Collider[] RawColliders => Colliders.Select((x) => x.Target).ToArray();
        public static ICollider[] m_CollidersArray = new ICollider[] { };
        public static bool HasSelection => Colliders.Count() > 0;
        public static bool HasMultiSelection => Colliders.Count() > 1;


        private static bool m_IsDirty;
        private static List<ICollider> s_Colliders = new List<ICollider>();

        static ColliderSelection()
        {
            ColliderTracker.trackedCollidersChanged += () => {validateColliders();};
            ColliderTracker.editorTrackerRebuilt += () => {m_IsDirty = true;};
        }

        //Removes disabled or not valid colliders and caching array
        //Called: after selection changed/after trackedCollidersChanged
        private static void validateColliders()
        {

            var result = ColliderTracker.EnabledColliders.Intersect(s_Colliders).ToArray();
            if (result.Count() != s_Colliders.Count)
            {
                s_Colliders.Clear();
                s_Colliders.AddRange(result);
            }

            if (m_CollidersArray.Length != s_Colliders.Count)
                m_CollidersArray = new ICollider[s_Colliders.Count];

            s_Colliders.CopyTo(m_CollidersArray);
            m_IsDirty = false;
            selectionChanged?.Invoke();
        }
    }
}
