using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.ShortcutManagement;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderCommands : ColliderEditorScriptableSingleton<ColliderCommands>
    {
        [Shortcut("Colliders Editor Tools/Paste Collider As New", typeof(SceneView))]
        private static void pasteColliderAsNew(ShortcutArguments args)
        {
            ColliderCommands.instance.Activate();
            (args.context as SceneView)?.SendEvent(EditorGUIUtility.CommandEvent("PasteColliderAsNew"));
            EditorApplication.delayCall += () => ColliderCommands.instance.Deactivate();
        }


        private List<ICollider> m_OpSrcColliders = new List<ICollider>();


        protected override void OnEnable()
        {
            base.OnEnable();
            EddieUtility.RegisterPrivateEvent<Selection, Action<int>>(null, "selectedObjectWasDestroyed", onSelectedObjectWasDestroyed);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EddieUtility.RemovePrivateEvent<Selection, Action<int>>(null, "selectedObjectWasDestroyed", onSelectedObjectWasDestroyed);

        }

        protected override void OnTrackedCollidersChanged() => cleanOpSrcColliders();

        protected override void OnSceneGUI(SceneView sceneView)
        {
            Event current = Event.current;
            if (current.type != EventType.ExecuteCommand)
                return;

            ICollider[] colliders = ColliderSelection.Colliders;
            bool shouldExecute = current.type == EventType.ExecuteCommand;
            switch (current.commandName)
            {
                case "PasteColliderAsNew":
                    doPasteAsNew(Selection.gameObjects);
                    break;
                case "Copy":
                    if (shouldExecute)
                        doCopy(colliders);
                    break;
                case "Paste":
                    if (shouldExecute)
                    {
                        if (colliders.Length > 0)
                            doPasteValues(colliders);
                        else
                            doPasteAsNew(Selection.gameObjects);
                    }
                    break;
                case "Duplicate":
                    if (shouldExecute)
                        doDuplicate(colliders);
                    break;
                case "Delete":
                case "SoftDelete":
                    if (shouldExecute)
                        doDelete(colliders);
                    break;
                case "FrameSelected":
                    if (shouldExecute)
                        doFrameColliders(sceneView, colliders);
                    break;
                case "FrameSelectedWithLock":
                    if (shouldExecute)
                        doFrameColliders(sceneView, colliders);
                    break;
                default:
                    return;
            }

            current.Use();
        }


        private void doCopy(ICollider[] colliders)
        {
            m_OpSrcColliders.Clear();

            if (colliders.Length == 0)
                return;

            m_OpSrcColliders.AddRange(colliders);
        }

        private void doPasteValues(ICollider[] opTargets)
        {
            var opSrcColliders = m_OpSrcColliders;
            int opSrcCollidersLen = m_OpSrcColliders.Count;
            int targetsLen = opTargets.Length;

            if (opSrcCollidersLen == 0 || targetsLen == 0)
                return;

            if (opSrcCollidersLen == 1)
            {
                 var opSrc = m_OpSrcColliders[0];
                if (!opSrc.IsTargetValid)
                    return;
               
                UnityEditorInternal.ComponentUtility.CopyComponent(opSrc.Target);
                foreach (var opTarget in opTargets)
                {
                    if (!opTarget.IsTargetValid)
                        continue;

                    if (opSrc.TargetType != opTarget.TargetType)    
                        continue;
 
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(opTarget.Target);
                }
            }
            else
            {
                int len = Mathf.Min(targetsLen, opSrcCollidersLen);
                for (int i = 0; i < len; i++)
                {
                    if (!m_OpSrcColliders[i].IsTargetValid || !opTargets[i].IsTargetValid)
                        continue;

                    if (m_OpSrcColliders[i].TargetType != opTargets[i].TargetType)
                        continue;

                    UnityEditorInternal.ComponentUtility.CopyComponent(m_OpSrcColliders[i].Target);
                    UnityEditorInternal.ComponentUtility.PasteComponentValues(opTargets[i].Target);
                }
            }
        }

        private void doPasteAsNew(GameObject[] targetGameObjects)
        {
            if (targetGameObjects == null || m_OpSrcColliders.Count == 0)
                return;

            var newColliders = ColliderTracker.PasteCollidersAsNew(m_OpSrcColliders.ToArray(), targetGameObjects);
            if (newColliders.Length > 0)
                ColliderSelection.Colliders = newColliders;
        }

        private void doDuplicate(ICollider[] colliders)
        {
            if (colliders.Length == 0)
                return;

            var newColliders = new List<ICollider>(colliders.Length);
            foreach (var collider in colliders)
            {
                if (!collider.IsTargetValid)
                    continue;
                var newCollider = ColliderTracker.PasteColliderAsNew(collider, collider.GameObject);
                if(newCollider != null)
                    newColliders.Add(newCollider);
            }

            if(newColliders.Count > 0)
                ColliderSelection.Colliders = newColliders.ToArray();
        }

        private void doDelete(ICollider[] colliders)
        {
            if (colliders.Length == 0)
                return;

            ColliderTracker.DestroyColliders(colliders);
        }

        private void doFrameColliders(SceneView sceneView, ICollider[] colliders)
        {
            if (colliders.Length == 0)
            {
                sceneView.FrameSelected();
                return;
            }

            var bounds = ColliderHandleUtility.GetCollidersWorldBounds(colliders);
            sceneView.SetViewIsLockedToObject(false);
            sceneView.Frame(bounds, false);
        }

        private void onSelectedObjectWasDestroyed(int instanceId) => cleanOpSrcColliders();

        private void cleanOpSrcColliders()
        {
            for (int i = m_OpSrcColliders.Count - 1; i >= 0; --i)
            {
                if (m_OpSrcColliders[i] == null)
                    m_OpSrcColliders.RemoveAt(i);
            }
        }


    }
}
