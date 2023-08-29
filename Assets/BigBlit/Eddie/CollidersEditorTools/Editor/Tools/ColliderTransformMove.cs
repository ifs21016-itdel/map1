using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;


namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderTransformMove : ColliderTransformTool
    {
        [Shortcut("Colliders Editor Tools/Move", typeof(SceneView))]
        private static void activateTransformMoveTool()
        {
            var toolManager = ColliderTransformToolManager.instance;
            if (toolManager.IsActive && toolManager.ActiveToolType == typeof(ColliderTransformMove))
                toolManager.SetActiveTool<ColliderTransformNone>();
            else
                ColliderTransformToolManager.instance.SetActiveTool<ColliderTransformMove>();
        }


        public override void OnTransformGUI(SceneView sceneView, Vector3 handlePosition, Quaternion handleRotation)
        {
            ICollider[] targets = Targets;

            EditorGUI.BeginChangeCheck();
            var positionHandleIds = HandlesEx.PositionHandleIds.@default;
            Vector3 newHandlePos = ColliderHandleUtility.DoPositionHandle(positionHandleIds, handlePosition, handleRotation);

            if (!EditorGUI.EndChangeCheck() || !IsTransforming)
                return;

            Vector3 minDragDifference = ColliderHandleUtility.GetMinDragDifference(newHandlePos);
            Vector3 worldDeltaPos = newHandlePos - handlePosition;
            handlePosition += worldDeltaPos;
            var selectedColliders = ColliderSelection.Colliders;

            foreach (var target in targets)
            {
                if (!target.IsTargetValid || !ColliderHandleUtility.IsLossyScaleValid(target))
                    continue;
      
                Undo.RecordObject(target.Target, "Move Selected Colliders");
                target.WorldCenter = target.WorldCenter + worldDeltaPos;

                if (!(EditorSnapSettingsEx.IncrementalSnapActive || EditorSnapSettingsEx.VertexSnapActive))
                    ColliderHandleUtility.RoundCenter(target, minDragDifference, new bool[]
                    {
                            !Mathf.Approximately(worldDeltaPos.x, 0.0f),
                             !Mathf.Approximately(worldDeltaPos.y, 0.0f),
                              !Mathf.Approximately(worldDeltaPos.z, 0.0f)
                    });
            }
            Undo.SetCurrentGroupName("Move Selected Colliders " + handlePosition.ToString());
        }

        private void drawTransfromingRectangle(ICollider target)
        {

        }
    }
}
