using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    [InitializeOnLoad]
    internal static class ColliderTracker
    {
        public static event Action trackedCollidersChanged;
        public static event Action editorTrackerRebuilt;

        public static ICollider[] Colliders
        {
            get
            {
                if (s_IsDirty)
                    updateColliders();
                return s_Colliders;
            }
        }

        public static ICollider[] EnabledColliders
        {
            get
            {
                if (s_IsDirty)
                    updateColliders();
                if (s_IsEnabledDirty)
                    updateEnabledColliders();
                return s_EnabledColliders;
            }
        }

        public static Collider[] RawColliders => Colliders.Select((x) => x.Target).ToArray();
        public static Collider[] EnabledRawColliders => EnabledColliders.Select((x) => x.Target).ToArray();


        private static ICollider[] s_Colliders = new ICollider[] { };
        private static ICollider[] s_EnabledColliders = new ICollider[] { };
        private static List<ICollider> s_CollidersList = new List<ICollider>();

        private static bool s_IsDirty;
        private static bool s_IsEnabledDirty;


        static ColliderTracker()
        {
            setDirty(true);
            registerTrackerRebuildEvent();
            EditorApplication.update += onUpdate;
        }

        public static ICollider CreateCollider(ICollider collider, GameObject gameObject)
        {
            var newCollider = gameObject.AddComponent(collider.TargetType);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            return ColliderUtility.CreateProxy(newCollider);
        }

        public static ICollider PasteColliderAsNew(ICollider srcCollider, GameObject targetGameObject)
        {
            var proxy = pasteColliderAsNew(srcCollider, targetGameObject);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            return proxy;
        }

        public static ICollider[] PasteCollidersAsNew(ICollider[] srcColliders, GameObject[] targetGameObjects)
        {
            var newColliders = new List<ICollider>(srcColliders.Length * targetGameObjects.Length);

            foreach (var targetGameObject in targetGameObjects)
            {
                if (targetGameObject == null)
                    continue;
                foreach (var srcCollider in srcColliders)
                {
                    if (srcCollider == null || !srcCollider.IsTargetValid)
                        continue;
                    newColliders.Add(pasteColliderAsNew(srcCollider, targetGameObject));
                }
            }
            if (newColliders.Count > 0)
                ActiveEditorTracker.sharedTracker.ForceRebuild();

            return newColliders.ToArray();
        }

        public static void DestroyCollider(ICollider collider)
        {
            if (!s_Colliders.Contains(collider))
                return;


            Undo.DestroyObjectImmediate(collider.Target);
            setDirty(true);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }

        public static void DestroyColliders(ICollider[] colliders)
        {
            var results = colliders.Intersect(s_Colliders);
            if (results == null)
                return;

            foreach (var collider in results)
            {

                if (collider.IsTargetValid)
                    Undo.DestroyObjectImmediate(collider.Target);
            }
            setDirty(true);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }


        private static void onUpdate() => setEnabledDirty(true);

        private static void registerTrackerRebuildEvent() => EddieUtility.RegisterPrivateEvent<ActiveEditorTracker, Action>(null, "editorTrackerRebuilt", new Action(onRegisterTrackerRebuildEvent));

        private static void onRegisterTrackerRebuildEvent()
        {
            updateColliders();
            //setDirty(true);
            editorTrackerRebuilt?.Invoke();
        }

        private static void updateColliders()
        {
            setDirty(false);

            var editors = ActiveEditorTracker.sharedTracker.activeEditors;
            s_CollidersList.Clear();
            foreach (var editor in editors)
            {
                if (editor == null)
                    continue;

                //TODO: Does the tracker track custom editor targets? If not, is it worth/need to implement it here?
                var targets = editor.targets;
                if (targets == null)
                    continue;


                foreach (var target in targets)
                {
                    var proxy = ColliderUtility.CreateProxy(target);
                    if (proxy == null)
                        continue;

                    s_CollidersList.Add(proxy);
                }
            }

            if (s_Colliders.Length != s_CollidersList.Count)
                s_Colliders = new ICollider[s_CollidersList.Count];
            s_CollidersList.CopyTo(s_Colliders);
            setEnabledDirty(true);
            trackedCollidersChanged?.Invoke();
        }

        private static void updateEnabledColliders()
        {
            setEnabledDirty(false);
            var colliders = s_Colliders;

            if (colliders.Length == 0)
            {
                if (s_EnabledColliders.Length != 0)
                {
                    s_EnabledColliders = new ICollider[] { };
                    trackedCollidersChanged?.Invoke();
                }
                return;
            }

            bool isDirty = false;

            var enabledList = s_CollidersList;
            enabledList.Clear();
            if (enabledList.Capacity < colliders.Length)
                enabledList.Capacity = colliders.Length;

            foreach (var collider in colliders)
            {
                if (collider.IsTargetValid && collider.Target.enabled)
                    enabledList.Add(collider);
            }

            if (enabledList.Count != s_EnabledColliders.Length)
            {
                isDirty = true;
            }
            else
            {
                //Get rid of LINQ
                var result = enabledList.Except(s_EnabledColliders);
                isDirty = result.Count() != 0;
            }

            if (isDirty)
            {
                if (s_EnabledColliders.Length != enabledList.Count)
                    s_EnabledColliders = new ICollider[enabledList.Count];
                enabledList.CopyTo(s_EnabledColliders);
                trackedCollidersChanged?.Invoke();
            }
        }

        private static void setDirty(bool isDirty) => s_IsDirty = isDirty;

        private static void setEnabledDirty(bool isDirty) => s_IsEnabledDirty = isDirty;

        private static ICollider pasteColliderAsNew(ICollider srcCollider, GameObject targetGameObject)
        {
            var collider = Undo.AddComponent(targetGameObject, srcCollider.TargetType);
            UnityEditorInternal.ComponentUtility.CopyComponent(srcCollider.Target);
            UnityEditorInternal.ComponentUtility.PasteComponentValues(collider);
            return ColliderUtility.CreateProxy(collider);
        }


    }
}
// 