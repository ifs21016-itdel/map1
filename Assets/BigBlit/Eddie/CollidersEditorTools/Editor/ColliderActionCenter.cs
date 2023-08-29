using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    [InitializeOnLoad]
    internal static class ColliderActionCenter
    {
        public static Vector3 Position
        {
            get
            {
                if (s_IsDirty && !m_lockActionCenter)
                    updateActionCenter();
                return s_Position;
            }
        }

        public static Quaternion Rotation => s_Rotation;
        public static void SetDirty() => s_IsDirty = true;

        public static bool LockActionCenter
        {
            get => m_lockActionCenter;
            set => m_lockActionCenter = value;
        }


        private static Quaternion s_Rotation;
        private static Vector3 s_Position;
        private static bool s_IsDirty;
        private static bool m_lockActionCenter;

        static ColliderActionCenter()
        {
#if UNITY_2021_1_OR_NEWER
            Tools.pivotModeChanged += onPivotModeChanged;
            Tools.pivotRotationChanged += onPivotRotationChanged;
#endif

            Undo.undoRedoPerformed += onUndoRedoPerformed;
            ColliderSelection.selectionChanged += onSelectionChanged;
            SceneView.duringSceneGui += onSceneGUI;
        }

        private static void updateActionCenter()
        {
            s_Rotation = ColliderHandleUtility.GetHandleRotation();
            s_Position = ColliderHandleUtility.GetHandlePosition();
            s_IsDirty = false;
        }

        private static void onSceneGUI(SceneView view)
        {
            if (Event.current.type == EventType.Repaint)
                SetDirty();
        }

        private static void onSelectionChanged() => SetDirty();
        private static void onUndoRedoPerformed() => SetDirty();
        private static void onPivotModeChanged() => SetDirty();
        private static void onPivotRotationChanged() => SetDirty();



    }
}

