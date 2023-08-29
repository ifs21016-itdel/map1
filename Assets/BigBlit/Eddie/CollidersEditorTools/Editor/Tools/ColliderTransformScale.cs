using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Collections.Generic;

namespace BigBlit.Eddie.CollidersEditorTools
{

    internal class ColliderTransformScale : ColliderTransformTool
    {
        private class ScaleTransformData
        {
            public ICollider target; //action targets
            public Vector3 position; //transform started world position
            public Quaternion rotation; //transform started rotation
            public Vector3 size; //transform started local size
        }
    

        private static Quaternion[] s_Alignments = new Quaternion[6]
           {
            Quaternion.LookRotation(Vector3.right, Vector3.up),
            Quaternion.LookRotation(Vector3.right, Vector3.forward),
            Quaternion.LookRotation(Vector3.up, Vector3.forward),
            Quaternion.LookRotation(Vector3.up, Vector3.right),
            Quaternion.LookRotation(Vector3.forward, Vector3.right),
            Quaternion.LookRotation(Vector3.forward, Vector3.up)
           };


        [Shortcut("Colliders Editor Tools/Scale", typeof(SceneView))]
        private static void activateTransformScaleTool()
        {
            var toolManager = ColliderTransformToolManager.instance;
            if (toolManager.IsActive && toolManager.ActiveToolType == typeof(ColliderTransformScale))
                toolManager.SetActiveTool<ColliderTransformNone>();
            else
                ColliderTransformToolManager.instance.SetActiveTool<ColliderTransformScale>();
        }


        private Vector3 m_Size = Vector3.one; //current handle size
        private List<ScaleTransformData> m_ScaleTransforms = new List<ScaleTransformData>();


        protected override void OnTransformStarted()
        {
            ColliderActionCenter.LockActionCenter = true;

            m_Size = Vector3.one;
            m_ScaleTransforms.Clear();
            var targets = Targets;
            if (targets.Length == 0)
                return;

            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (!target.IsTargetValid)
                    continue;

                m_ScaleTransforms.Add(new ScaleTransformData()
                {
                    target = target,
                    position = target.WorldCenter,
                    rotation = target.Transform.rotation,
                    size = target.HandleSize
                });

            }
        }

        protected override void OnTransformFinished()
        {
            ColliderActionCenter.LockActionCenter = false;
            m_Size = Vector3.one;
            m_ScaleTransforms.Clear();
        }

        public override void OnTransformGUI(SceneView sceneView, Vector3 handlePosition, Quaternion handleRotation)
        {
            EditorGUI.BeginChangeCheck();

            var newSize = Handles.ScaleHandle(m_Size, handlePosition, handleRotation, HandleUtility.GetHandleSize(handlePosition));

            if (!EditorGUI.EndChangeCheck() || !IsTransforming || m_ScaleTransforms.Count == 0)
                return;

            Vector3 pivotPos = handlePosition;
            Quaternion pivotRot = handleRotation;
            Vector3 minDragDifference = ColliderHandleUtility.GetMinDragDifference(handlePosition);
            newSize = new Vector3(Mathf.Abs(newSize.x), Mathf.Abs(newSize.y), Mathf.Abs(newSize.z));
            Vector3 deltaSize = (newSize - m_Size);
            m_Size = newSize;

            foreach (var scaleTransform in m_ScaleTransforms)
            {
                if (!scaleTransform.target.IsTargetValid)
                    continue;

                ICollider collider = scaleTransform.target;
                if (!ColliderHandleUtility.IsLossyScaleValid(collider))
                    continue;

                Undo.RecordObject(collider.Target, "Scale Selected Collider");

                if (Tools.pivotMode == PivotMode.Pivot)
                    pivotPos = scaleTransform.position;

                if (Tools.pivotRotation == PivotRotation.Local && Tools.pivotMode == PivotMode.Pivot)
                    pivotRot = scaleTransform.rotation;

                var scaledPosDelta = pivotRot * Vector3.Scale(Quaternion.Inverse(pivotRot) * (scaleTransform.position - pivotPos), (Vector3)deltaSize);
                scaleTransform.target.WorldCenter = scaleTransform.target.WorldCenter + scaledPosDelta;

                if (!(EditorSnapSettingsEx.IncrementalSnapActive || EditorSnapSettingsEx.VertexSnapActive))
                    ColliderHandleUtility.RoundCenter(collider, minDragDifference, new bool[]
                   {
                            !Mathf.Approximately(scaledPosDelta.x, 0.0f),
                             !Mathf.Approximately(scaledPosDelta.y, 0.0f),
                              !Mathf.Approximately(scaledPosDelta.z, 0.0f)
                   });

                Quaternion colliderRot = scaleTransform.rotation;
                var deltaSizeRotAlign = getDeltaSizeRotationAlignment(pivotRot, colliderRot);
                var deltaSizeAligned = deltaSizeRotAlign * deltaSize;
                deltaSizeAligned = Vector3.Scale(deltaSizeAligned, deltaSizeRotAlign * Vector3.one);
                deltaSizeAligned = Vector3.Scale(scaleTransform.size, deltaSizeAligned);


                scaleTransform.target.HandleSize = scaleTransform.target.HandleSize + deltaSizeAligned;
            }

            Undo.SetCurrentGroupName("Scale Selected Colliders " + handlePosition.ToString());
        }

        private Quaternion getDeltaSizeRotationAlignment(Quaternion targetRotation, Quaternion ownRotation)
        {
            float num1 = float.NegativeInfinity;
            Quaternion refAlignment = Quaternion.identity;
            for (int index = 0; index < s_Alignments.Length; ++index)
            {
                float num2 = Mathf.Min(Mathf.Abs(Vector3.Dot(targetRotation * Vector3.right,
                 ownRotation * s_Alignments[index] * Vector3.right)),
                 Mathf.Abs(Vector3.Dot(targetRotation * Vector3.up, ownRotation * s_Alignments[index] * Vector3.up)),
                 Mathf.Abs(Vector3.Dot(targetRotation * Vector3.forward,
                  ownRotation * s_Alignments[index] * Vector3.forward)));
                if ((double)num2 > (double)num1)
                {
                    num1 = num2;
                    refAlignment = s_Alignments[index];
                }
            }
            return refAlignment;
        }
    }
}
