using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;


namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderTransformToolManager : ColliderToolManager<ColliderTransformToolManager, ColliderTransformTool>
    {
        protected override Type NoneToolType => typeof(ColliderTransformNone);


        private Vector2 m_CurMousePos;
        private bool m_TargetsDirty;


        protected override void OnSceneGUI(SceneView sceneView)
        {

            hookShortcuts();

            var targets = ColliderSelection.Colliders;

            if (targets.Length == 0)
                return;

            if (m_TargetsDirty)
            {
                ActiveTool.Targets = targets;
                m_TargetsDirty = false;
            }

            var current = Event.current;
            var prevDelta = current.delta;
            var prevPos = current.mousePosition;

            if (ActiveTool.IsTransforming)
            {
                current.delta = current.shift ? current.delta * ColliderPreferences.instance.PrecideModeFactor : current.delta;
                m_CurMousePos += current.delta;
                current.mousePosition = m_CurMousePos;
            }

            var handlePosition = ColliderActionCenter.Position;
            var handleRotation = ColliderActionCenter.Rotation;
            var eventType = Event.current.GetTypeForControl(ActiveTool.HotControlId);
            var hotControl = GUIUtility.hotControl;

            ActiveTool.OnTransformGUI(sceneView, handlePosition, handleRotation);
            if (ActiveTool.TransformStageCheck(eventType, hotControl))
            {
                if (ActiveTool.IsTransforming)
                    OnToolTransformStarted();
                else
                    OnToolTransformFinished();
            }

            current.delta = prevDelta;
            current.mousePosition = prevPos;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            setTargetsDirty();
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }

        protected override void OnToolActivating()
        {
            ActiveTool.Targets = ColliderSelection.Colliders;
            if (ActiveTool.IsTransforming)
                OnToolTransformStarted();

        }

        protected override void OnToolWillBeDeactivated()
        {
            if (ActiveTool.IsTransforming)
                OnToolTransformFinished();
        }

        protected override void OnTrackedCollidersChanged() => setTargetsDirty();

        protected override void OnSelectedCollidersChanged() => setTargetsDirty();

        protected virtual void OnToolTransformStarted() => m_CurMousePos = Event.current.mousePosition;

        protected virtual void OnToolTransformFinished() { }


        private void hookShortcuts()
        {
            if (!ColliderPreferences.instance.HookShortcuts.Value)
                return;

            var current = Event.current;
            if (current.button != 0 || UnityEditor.Tools.viewToolActive)
                return;

            if (current.type == EventType.KeyDown)
            {
                if (getHookedType(current) != null)
                    current.Use();
            }
            else if (current.type == EventType.KeyUp)
            {
                var toolType = getHookedType(current);
                if (toolType == null)
                    return;
                if (ActiveToolType == toolType)
                    SetActiveTool(typeof(ColliderTransformNone));
                else
                    SetActiveTool(toolType);
                current.Use();
            }
        }

        private Type getHookedType(Event evt)
        {
            if (isBindingKey(evt, "Tools/Move"))
                return typeof(ColliderTransformMove);
            if (isBindingKey(evt, "Tools/Scale"))
                return typeof(ColliderTransformScale);
            if (isBindingKey(evt, "Tools/Rotate"))
                return typeof(ColliderTransformRotate);
            if (isBindingKey(evt, "Tools/Rect"))
                return typeof(ColliderTransformBounds);
            return null;
        }

        private bool isBindingKey(Event evt, string shortcutId)
        {
            var binding = ShortcutManager.instance.GetShortcutBinding(shortcutId);
            if (binding.keyCombinationSequence == null || binding.keyCombinationSequence.Count() == 0)
                return false;

            var keyComb = binding.keyCombinationSequence.First();
            return keyComb.keyCode == evt.keyCode && keyComb.shift == evt.shift && keyComb.alt == evt.alt && keyComb.action == evt.control;
        }

        private void setTargetsDirty() => m_TargetsDirty = true;

    }
}
