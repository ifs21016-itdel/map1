using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.ShortcutManagement;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderTransformRotate : ColliderTransformTool
    {
        [Shortcut("Colliders Editor Tools/Rotate", typeof(SceneView))]
        private static void activateTransformScaleTool()
        {
            var toolManager = ColliderTransformToolManager.instance;
            if (toolManager.IsActive && toolManager.ActiveToolType == typeof(ColliderTransformRotate))
                toolManager.SetActiveTool<ColliderTransformNone>();
            else
                ColliderTransformToolManager.instance.SetActiveTool<ColliderTransformRotate>();
        }


        Quaternion m_CurRotation;


        public override void OnTransformGUI(SceneView sceneView, Vector3 handlePosition, Quaternion handleRotation)
        {
            var handleRot = Targets[0].Transform.rotation;

            using var scope = new Handles.DrawingScope(Matrix4x4.TRS(handlePosition, handleRot, Vector3.one));
            EditorGUI.BeginChangeCheck();
            Handles.lighting = true;
            var newRotation = Handles.RotationHandle(Quaternion.identity, Vector3.zero);
            if (!EditorGUI.EndChangeCheck())
                return;

            var deltaAngles = (newRotation * Quaternion.Inverse(m_CurRotation));
            deltaAngles.ToAngleAxis(out var angle, out var axis);
            bool rx = Mathf.Abs(angle * axis.x) >= 90.0f;
            bool ry = Mathf.Abs(angle * axis.y) >= 90.0f;
            bool rz = Mathf.Abs(angle * axis.z) >= 90.0f;
            if (rx || ry || rz)
            {
                ; m_CurRotation = newRotation;
                Vector3 rotAxis = new Vector3(rx ? Mathf.Sign(axis.x) * 1.0f : 0.0f,
                    ry ? Mathf.Sign(axis.y) * 1.0f : 0.0f,
                    rz ? Mathf.Sign(axis.z) * 1.0f : 0.0f);
                Undo.RecordObjects(Targets.Select((x) => x.Target).ToArray(), "Colliders Rotate " + Targets.Length);
                Quaternion rotQuat = Quaternion.Euler(rotAxis * 90.0f);
                foreach (var target in Targets)
                {
                    if (!ColliderHandleUtility.IsLossyScaleValid(target))
                        continue;
                    target.Rotate(rotAxis);
                    if (Tools.pivotMode == PivotMode.Center)
                        target.WorldCenter = Handles.matrix.MultiplyPoint(rotQuat * Handles.inverseMatrix.MultiplyPoint(target.WorldCenter));

                }


            }
        }


        protected override void OnTransformStarted()
        {
            m_CurRotation = Quaternion.identity;
            ColliderActionCenter.LockActionCenter = true;
        }

        protected override void OnTransformFinished() => ColliderActionCenter.LockActionCenter = false;

    }
}
