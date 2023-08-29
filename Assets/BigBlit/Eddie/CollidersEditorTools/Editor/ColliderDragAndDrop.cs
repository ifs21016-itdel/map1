using System;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderDragAndDrop : ColliderEditorScriptableSingleton<ColliderDragAndDrop>
    {

#if UNITY_2021_2_OR_NEWER

        protected override void OnActivated() => DragAndDrop.AddDropHandler(onSceneDrop);

        protected override void OnDeactivated() => DragAndDrop.RemoveDropHandler(onSceneDrop);

        private DragAndDropVisualMode onSceneDrop(UnityEngine.Object dropUpon, Vector3 worldPosition, Vector2 viewportPosition, Transform parentForDraggedObjects, bool perform)
        {
            return processDragAndDrop(dropUpon, perform);
        }

#else
        protected override void OnSceneGUI(SceneView sceneView)
        {
            Event current = Event.current;
            EventType eventType = current.type;
            switch (eventType)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = processDragAndDrop(HandleUtility.PickGameObject(current.mousePosition, false), false);
                    current.Use();
                    break;

                case EventType.DragPerform:
                    DragAndDrop.visualMode = processDragAndDrop(HandleUtility.PickGameObject(current.mousePosition, false), true);
                    if (DragAndDrop.visualMode != DragAndDropVisualMode.None)
                        DragAndDrop.AcceptDrag();
                    current.Use();
                    break;
                case EventType.DragExited:
                    current.Use();
                    break;
            }
        }

#endif

        private DragAndDropVisualMode processDragAndDrop(UnityEngine.Object dropUpon, bool perform)
        {
            var targets = ColliderTracker.EnabledColliders;
            if (targets.Length == 0 || DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return DragAndDropVisualMode.None;

            var srcRef = DragAndDrop.objectReferences[0] as PhysicMaterial;
            if (srcRef == null)
                return DragAndDropVisualMode.None;

            var collider = ColliderHandleUtility.GetClosestColliderAtGUIPos(Event.current.mousePosition, targets, out var hitPoint);
            if (collider == null || !collider.IsTargetValid)
            {
                var go = dropUpon as GameObject;
                if (go != null)
                {
                    var colliders = go.GetComponents<Collider>();
                    if (colliders != null)
                        colliders = colliders.Where((x) => x.enabled).ToArray();
                    if (colliders == null)
                        return DragAndDropVisualMode.None;

                    if (perform)
                    {
                        Undo.RecordObjects(colliders, "Set Colliders Material: " + colliders.Length);
                        foreach (var c in colliders)
                        {
                            if (c != null)
                                c.sharedMaterial = srcRef;
                        }
                    }

                    return DragAndDropVisualMode.Link;
                }
                return DragAndDropVisualMode.None;
            }

            if (perform)
            {
                var selectedColliders = ColliderSelection.Colliders;
                if (selectedColliders.Contains(collider))
                {
                    var colliders = selectedColliders.Where((x) => x.IsTargetValid).Select((x) => x.Target).ToArray();
                    if (colliders == null || colliders.Length == 0)
                        return DragAndDropVisualMode.None;

                    Undo.RecordObjects(colliders, "Set Colliders Material: " + colliders.Length);
                    foreach (var c in colliders)
                        c.sharedMaterial = srcRef;
                }
                else
                {
                    if (!collider.IsTargetValid)
                        return DragAndDropVisualMode.None;

                    Undo.RecordObject(collider.Target, "Set Collider Material: " + collider.Target.name);
                    collider.Target.sharedMaterial = srcRef;
                }
            }
            return DragAndDropVisualMode.Link;
        }
    }
}
