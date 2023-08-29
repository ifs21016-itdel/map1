using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderSelector : ColliderEditorScriptableSingleton<ColliderSelector>
    {
        [ClutchShortcut("Colliders Editor Tools/Select Colliders")]
        static void OnSelectCollider(ShortcutArguments args)
        {
            if (args.stage == ShortcutStage.Begin)
                ColliderSelector.instance.Activate();
            else
                ColliderSelector.instance.Deactivate();
        }

        public static event Action hoveredColliderChanged;
        public static event Action colliderClicked;
        public static event Action colliderSelected;

        private static int s_SelectorIdHash = "ColliderSelector".GetHashCode();


        private Color HoveredColliderColor => ColliderPreferences.instance.HoveredColliderColor;
        private Color SelectedColliderColor => ColliderPreferences.instance.SelectedColliderColor;
        private bool ShowCenterlines => ColliderPreferences.instance.ShowCenterlines;
        private float LinesThickness => ColliderPreferences.instance.LinesThickness;
        private bool CenterLinesAxisColorized => ColliderPreferences.instance.CenterLinesAxesColors;

        private bool HasHoveredCollider => m_HoveredCollider != null;
        private ICollider HoveredCollider => m_HoveredCollider;
        private ICollider m_HoveredCollider;

        private Vector2 m_DownPosition;
        private bool m_IsRectSelecting;
        private ICollider[] m_StartRectSelection;
        private ICollider[] m_CurrentRectSelection;
        private HashSet<ICollider> m_PrevRectSelection = new HashSet<ICollider>();

        private SceneView m_CurrentSceneView;

        private static int m_SelectorId = -1;

        protected override void OnSceneGUI(SceneView sceneView)
        {
            Event current = Event.current;
            int selectorId = m_SelectorId;
            EventType eventType = current.GetTypeForControl(selectorId);
            var targets = ColliderTracker.EnabledColliders;
            m_CurrentSceneView = sceneView;

            if (targets.Length == 0)
            {
                if (GUIUtility.hotControl == selectorId)
                {
                    GUIUtility.hotControl = 0;
                    if (m_IsRectSelecting)
                    {
                        finishRectSelecting();
                    }
                }
                return;
            }

            switch (eventType)
            {
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    doCommands(current, selectorId, targets);
                    break;
                case EventType.MouseDown:
                    doMouseDown(current, selectorId, targets);
                    break;
                case EventType.MouseUp:
                    doMouseUp(current, selectorId, targets);
                    break;
                case EventType.MouseDrag:
                    doMouseDrag(current, selectorId, targets);
                    break;
                case EventType.MouseMove:
                    doMouseMove(current, selectorId, targets);
                    break;
                case EventType.Layout:
                    doLayout(current, selectorId, targets);
                    break;
                case EventType.Repaint:
                    doRepaint(current, selectorId);
                    break;

            }
        }

        protected override void OnDeactivated()
        {
            if (m_IsRectSelecting)
                finishRectSelecting();

        }

        protected override void OnSelectedCollidersChanged() => SceneView.RepaintAll();

        protected override void OnTrackedCollidersChanged() => SceneView.RepaintAll();

        protected override void OnEnable()
        {
            base.OnEnable();
            m_SelectorId = GUIUtilityEx.GetPermanentControlID();
        }

        private void doMouseDown(Event current, int selectorId, ICollider[] targets)
        {
            if (HandleUtility.nearestControl != selectorId || current.alt || current.button != 0)
                return;

            m_DownPosition = current.mousePosition;
            m_IsRectSelecting = false;
            GUIUtility.hotControl = selectorId;
        }

        private void doMouseUp(Event current, int selectorId, ICollider[] targets)
        {
            if (GUIUtility.hotControl != selectorId || current.button != 0)
                return;

            if (m_IsRectSelecting)
                finishRectSelecting();
            else
                processClick(current, selectorId, targets);

            m_StartRectSelection = null;
            GUIUtility.hotControl = 0;
            current.Use();
        }

        private void doMouseDrag(Event current, int selectorId, ICollider[] targets)
        {
            if (GUIUtility.hotControl != selectorId)
                return;

            if (m_IsRectSelecting)
                processRectSelecting(current, selectorId, targets);
            else
            {
                if ((m_DownPosition - current.mousePosition).magnitude > 5.0f)
                {
                    m_IsRectSelecting = true;
                    startRectSelecting(current, selectorId, targets);
                }
            }
            current.Use();
        }

        private void doMouseMove(Event current, int selectorId, ICollider[] targets)
        {
            registerSelectorId(current, selectorId);
            updateHoveredCollider(current, selectorId, targets);
        }

        private void doLayout(Event current, int selectorId, ICollider[] targets)
        {
            registerSelectorId(current, selectorId);
            updateHoveredCollider(current, selectorId, targets);

            if (m_IsRectSelecting && GUIUtility.hotControl != selectorId)
                finishRectSelecting();
        }

        private void doRepaint(Event current, int selectorId)
        {
            doPaintHover(current, selectorId);
            doPaintSelectionFrame(current, selectorId);
            doRectSelectionFrame(current, selectorId);
        }

        private void doCommands(Event current, int selectorId, ICollider[] colliders)
        {
            bool shouldExecute = current.type == EventType.ExecuteCommand;
            switch (current.commandName)
            {
                case "ModifierKeysChanged":
                    if (shouldExecute)
                    {
                        if (GUIUtility.hotControl == selectorId && m_IsRectSelecting)
                            updateRectSelection(current, m_CurrentRectSelection);
                    }
                    break;

                case "SelectAll":
                    if (shouldExecute)
                        ColliderSelection.Colliders = colliders;
                    break;
                case "InvertSelection":
                    if (shouldExecute)
                    {
                        var invColliders = colliders.Except(ColliderSelection.Colliders).ToArray();
                        ColliderSelection.Colliders = invColliders;
                    }
                    break;
                case "DeselectAll":
                    if (shouldExecute)
                        ColliderSelection.Clear();
                    break;
                default:
                    return;
            }

            current.Use();
            if (shouldExecute)
                HandleUtility.Repaint();
        }

        private void processClick(Event current, int selectorId, ICollider[] targets)
        {
            var target = HoveredCollider;

            if (target != null)
            {
                colliderClicked?.Invoke();

                if (current.shift || EditorGUI.actionKey)
                {
                    if (ColliderSelection.Colliders.Contains(target))
                        ColliderSelection.Remove(target);
                    else
                    {
                        ColliderSelection.Add(target);
                        colliderSelected?.Invoke();
                    }
                }
                else
                {
                    ColliderSelection.Collider = target;
                    colliderSelected?.Invoke();
                }


            }
            else if (!(current.shift || EditorGUI.actionKey))
            {
                if (ColliderSelection.HasSelection)
                {
                    ColliderSelection.Colliders = null;
                }
                else
                {
                    ToolManager.RestorePreviousPersistentTool();
                }
            }
        }

        private void startRectSelecting(Event current, int selectorId, ICollider[] targets)
        {
            m_StartRectSelection = ColliderSelection.Colliders;
            m_CurrentRectSelection = null;
            if (m_PrevRectSelection == null)
                m_PrevRectSelection = new HashSet<ICollider>();
            else
                m_PrevRectSelection.Clear();
            m_IsRectSelecting = true;

            EditorApplication.modifierKeysChanged += sendCommandsOnModifierKeys;

        }

        private void processRectSelecting(Event current, int selectorId, ICollider[] targets)
        {
            var currentSelections = getCollidersInRect(ColliderHandleUtility.ToRect(m_DownPosition, current.mousePosition), targets);
            if (!hasCurrentSelectionChanged(currentSelections))
                return;

            m_PrevRectSelection.Clear();
            foreach (var c in currentSelections)
                m_PrevRectSelection.Add(c);

            updateRectSelection(current, currentSelections);
        }

        private void finishRectSelecting()
        {
            if (m_CurrentRectSelection != null && m_CurrentRectSelection.Length > 0)
                colliderSelected?.Invoke();

            m_CurrentRectSelection = null;
            EditorApplication.modifierKeysChanged -= sendCommandsOnModifierKeys;
            m_IsRectSelecting = false;
        }

        private bool hasCurrentSelectionChanged(ICollider[] currentSelections)
        {
            if (currentSelections.Length != m_PrevRectSelection.Count)
                return true;

            foreach (var c in currentSelections)
            {
                if (!m_PrevRectSelection.Contains(c))
                    return true;
            }

            return false;
        }

        private void updateRectSelection(Event current, ICollider[] currentSelection)
        {
            m_CurrentRectSelection = currentSelection;
            if (current.shift)
            {
                ColliderSelection.Clear();
                ColliderSelection.Add(m_StartRectSelection);
                ColliderSelection.Add(m_CurrentRectSelection);
            }
            else if (EditorGUI.actionKey)
            {
                ColliderSelection.Clear();
                ColliderSelection.Add(m_StartRectSelection);
                ColliderSelection.Remove(m_CurrentRectSelection);
            }
            else
            {
                ColliderSelection.Colliders = m_CurrentRectSelection;
            }
        }

        private void updateHoveredCollider(Event current, int selectorId, ICollider[] targets)
        {
            var collider = ColliderHandleUtility.GetNextColliderAtGUIPos(current.mousePosition, ColliderSelection.Collider, targets, out var hitPoint);
            if (collider != m_HoveredCollider)
            {
                m_HoveredCollider = collider;
                onHoverChanged(m_HoveredCollider);
            }
        }

        private void registerSelectorId(Event current, int selectorId)
        {
            if (!Tools.viewToolActive)
                HandleUtility.AddDefaultControl(selectorId);
        }

        private void onHoverChanged(ICollider hoveredCollider)
        {
            hoveredColliderChanged?.Invoke();
            HandleUtility.Repaint();
        }

        private void doPaintHover(Event current, int selectorId)
        {

            if (!HasHoveredCollider || ColliderSelection.Colliders.Contains(HoveredCollider) || (GUIUtility.hotControl != selectorId && (current.alt || current.button != 0))
                || (GUIUtility.hotControl != 0 && GUIUtility.hotControl != selectorId) || HandleUtility.nearestControl != selectorId)
                return;

            if (Mathf.Approximately(HoveredColliderColor.a, 0.0f))
                return;

            var collider = HoveredCollider;
            using var scope = new Handles.DrawingScope(HoveredColliderColor,
             Matrix4x4.TRS(collider.Transform.position,
             collider.Transform.rotation, Vector3.one));
            ColliderHandleUtility.DrawColliderFrame(collider, false, false, LinesThickness);
        }

        private void doPaintSelectionFrame(Event current, int selectorId)
        {
            if (!ColliderSelection.HasSelection)
                return;

            GameObject go = null;
            var prevMatrix = Handles.matrix;
            var prevColor = Handles.color;
            Handles.color = SelectedColliderColor;
            ForEachSelectedCollider((c) =>
            {
                if (go != c.GameObject)
                {
                    go = c.GameObject;
                    Handles.matrix = Matrix4x4.TRS(go.transform.position, go.transform.rotation, Vector3.one);
                }
                ColliderHandleUtility.DrawColliderFrame(c, ShowCenterlines, CenterLinesAxisColorized, LinesThickness);
            });

            Handles.matrix = prevMatrix;
            Handles.color = prevColor;
        }

        private void doRectSelectionFrame(Event current, int selectorId)
        {
            if (!m_IsRectSelecting)
                return;

            Handles.BeginGUI();
            EditorStylesEx.SelectionRect.Draw(ColliderHandleUtility.ToRect(m_DownPosition, current.mousePosition), GUIContent.none, false, false, false, false);
            Handles.EndGUI();
        }

        private ICollider[] getCollidersInRect(Rect rect, ICollider[] colliders)
        {
            ICollider[] prevColliders = m_CurrentRectSelection;

            List<ICollider> collidersInRect = new List<ICollider>();
            if (prevColliders == null)
                prevColliders = new ICollider[] { };

            foreach (var collider in prevColliders)
            {
                Vector3 center = collider.WorldCenter;
                Vector2 point = HandleUtility.WorldToGUIPoint(center);
                if (rect.Contains(point))
                {
                    collidersInRect.Add(collider);
                }
            }

            var newColliders = colliders.Except(prevColliders);
            foreach (var collider in newColliders)
            {
                Vector3 center = collider.WorldCenter;
                Vector2 point = HandleUtility.WorldToGUIPoint(center);
                if (rect.Contains(point))
                {
                    collidersInRect.Add(collider);
                }
            }

            return collidersInRect.ToArray();
        }

        private void sendCommandsOnModifierKeys() => m_CurrentSceneView.SendEvent(EditorGUIUtility.CommandEvent("ModifierKeysChanged"));
    }
}