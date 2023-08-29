using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderTransformBounds : ColliderTransformTool
    {
        [Shortcut("Colliders Editor Tools/Edit Bounds", typeof(SceneView))]
        private static void activateTransformBounds(ShortcutArguments args)
        {
            var toolManager = ColliderTransformToolManager.instance;
            if (toolManager.IsActive && toolManager.ActiveToolType == typeof(ColliderTransformBounds))
                toolManager.SetActiveTool<ColliderTransformNone>();
            else
                ColliderTransformToolManager.instance.SetActiveTool<ColliderTransformBounds>();
        }

        [ClutchShortcut("Colliders Editor Tools/Edit Bounds - Copy", typeof(SceneView))]
        private static void moveCopy(ShortcutArguments args)
        {
            if (args.stage == ShortcutStage.Begin)
                m_MoveCopyModifier = true;
            else
                m_MoveCopyModifier = false;
        }

        private readonly int[] m_ControlIdsHashes = new int[] {
            "ColliderTransformBoundsLeft".GetHashCode(), "ColliderTransformBoundsRight".GetHashCode(),
            "ColliderTransformBoundsDown".GetHashCode(), "ColliderTransformBoundsUp".GetHashCode(),
            "ColliderTransformBoundsBack".GetHashCode(), "ColliderTransformBoundsForward".GetHashCode()
        };

        private bool BoundsHandleAxesColors => ColliderPreferences.instance.BoundsHandleAxesColors;

        private static bool m_MoveCopy;
        private static bool m_MoveCopyModifier;

        protected override void OnTransformStarted()
        {

            if (m_MoveCopyModifier)
            {
                m_MoveCopy = true;
                m_MoveCopyModifier = false;
            }
        }

        protected override void OnTransformFinished()
        {
            m_MoveCopy = false;
            m_MoveCopyModifier = false;
        }

        public override void OnTransformGUI(SceneView sceneView, Vector3 handlePosition, Quaternion handleRotation)
        {
            foreach (var target in Targets)
            {
                if (!ColliderHandleUtility.IsLossyScaleValid(target))
                    continue;

                drawEditBounds(sceneView, target);
            }
        }


        private int[] getControlIds()
        {
            int[] controlIds = new int[6];
            for (int i = 0; i < 6; i++)
                controlIds[i] = GUIUtility.GetControlID(m_ControlIdsHashes[i], FocusType.Passive);
            return controlIds;
        }

        private void drawEditBounds(SceneView sceneView, ICollider collider)
        {

            int[] controlIds = getControlIds();

            using var scope = new Handles.DrawingScope(ColliderPreferences.instance.BoundsHandleColor,
              Matrix4x4.TRS(collider.Transform.position,
              collider.Transform.rotation, Vector3.one));

            Handles.CapFunction handleCap = Handles.CircleHandleCap;
            switch (ColliderPreferences.instance.BoundsHandlesType.Value)
            {
                case ColliderPreferences.HandleTypes.Cube:
                    handleCap = Handles.CubeHandleCap;
                    break;
                case ColliderPreferences.HandleTypes.Dot:
                    handleCap = Handles.DotHandleCap;
                    break;
                case ColliderPreferences.HandleTypes.Rectangle:
                    handleCap = Handles.RectangleHandleCap;
                    break;
            }

            var bounds = collider.HandleBounds;
            var center = bounds.center;
            var size = bounds.size;

            Vector3 handleCamPos = Handles.inverseMatrix.MultiplyPoint(sceneView.camera.transform.position);
            Vector3 handleCamDir = Handles.inverseMatrix.MultiplyVector(sceneView.camera.transform.forward);

            bool isCameraInside = bounds.Contains(handleCamPos);

            float handleSize = ColliderPreferences.instance.BoundsHandleSize * 0.25f;
            float snapping = EditorSnapSettings.scale;
            bool isOrtho = sceneView.camera.orthographic;
            var xColor = BoundsHandleAxesColors ? Handles.xAxisColor : Handles.color;
            var yColor = BoundsHandleAxesColors ? Handles.yAxisColor : Handles.color;
            var zColor = BoundsHandleAxesColors ? Handles.zAxisColor : Handles.color;

            EditorGUI.BeginChangeCheck();

            float minx = drawMidpointHandle(controlIds[0], new Vector3(bounds.min.x, center.y, center.z),
                 Vector3.left, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, xColor).x;
            float maxx = drawMidpointHandle(controlIds[1], new Vector3(bounds.max.x, center.y, center.z),
                 Vector3.right, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, xColor).x;
            float miny = drawMidpointHandle(controlIds[2], new Vector3(center.x, bounds.min.y, center.z),
                 Vector3.down, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, yColor).y;
            float maxy = drawMidpointHandle(controlIds[3], new Vector3(center.x, bounds.max.y, center.z),
                 Vector3.up, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, yColor).y;
            float minz = drawMidpointHandle(controlIds[4], new Vector3(center.x, center.y, bounds.min.z),
                 Vector3.back, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, zColor).z;
            float maxz = drawMidpointHandle(controlIds[5], new Vector3(center.x, center.y, bounds.max.z),
                 Vector3.forward, handleSize, handleCap, snapping, handleCamPos, handleCamDir, isOrtho, zColor).z;
            if (EditorGUI.EndChangeCheck())
            {
                if (m_MoveCopy)
                {
                    m_MoveCopy = false;
                    int hotControl = GUIUtility.hotControl;
                    UnityEditorInternal.ComponentUtility.CopyComponent(collider.Target);
                    var newCol = ColliderTracker.PasteColliderAsNew(collider, collider.GameObject);

                    var min = new Vector3(minx, miny, minz);
                    var max = new Vector3(maxx, maxy, maxz);

                    if (hotControl == controlIds[0])
                        max[0] = bounds.min.x;
                    else if (hotControl == controlIds[1])
                        min[0] = bounds.max.x;
                    else if (hotControl == controlIds[2])
                        max[1] = bounds.min.y;
                    else if (hotControl == controlIds[3])
                        min[1] = bounds.max.y;
                    else if (hotControl == controlIds[4])
                        max[2] = bounds.min.z;
                    else if (hotControl == controlIds[5])
                        min[2] = bounds.max.z;

                    newCol.SetHandleMinMax(min, max);
                    ColliderSelection.Collider = newCol;
                }
                else
                {
                    Undo.RecordObject(collider.Target, "Edit Selected Collider Bounds " + collider.Target.GetInstanceID());
                    var min = new Vector3(minx, miny, minz);
                    var max = new Vector3(maxx, maxy, maxz);
                    collider.SetHandleMinMax(min, max);
                }
            }
        }

        private Vector3 drawMidpointHandle(int controlId, Vector3 position, Vector3 direction, float handleSize, Handles.CapFunction handleCap, float snapping, Vector3 handleCamPos, Vector3 handleCamDir, bool isOrtho, Color color)
        {
            var prevColor = Handles.color;

            var d = Vector3.Dot(-handleCamDir, direction);
            var d2 = Mathf.Abs(d);

            if (isOrtho && d2 > 0.99999f)
                return position;

            var d3 = (d < 0.0f) ? (1.0f + d) : 1.0f;

            Handles.color = new Color(color.r, color.g, color.b, color.a * d3);

            if (isOrtho && d2 < 0.0005f && (handleCap == Handles.RectangleHandleCap || handleCap == Handles.CircleHandleCap))
                handleCap = Handles.DotHandleCap;

            var pos = Handles.Slider(controlId, position, direction, HandleUtility.GetHandleSize(position) * handleSize, handleCap, snapping);
            if (ToolsEx.VertexDragging && pos != position)
            {
                if (HandleUtility.FindNearestVertex(Event.current.mousePosition, out var vertex))
                    pos = Handles.inverseMatrix.MultiplyPoint(vertex);
            }
            Handles.color = prevColor;
            return pos;
        }
    }
}
