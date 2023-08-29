using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace BigBlit.Eddie.CollidersEditorTools
{

    [EditorTool("Colliders Transform Tools", typeof(Collider))]
    public class ColliderEditorTransformTools : EditorTool
    {
        private static GUIContent s_editModeButton;

        private static GUIContent EditModeButton => s_editModeButton ??= new GUIContent(
            EditorGUIUtility.isProSkin ? ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/d_CollidersEditorToolsIcon.png") :
             ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/l_CollidersEditorToolsIcon.png"),
            EditorGUIUtility.TrTextContent("Collider Editor Tools").text);

        public bool IsActive => m_IsActive;
        public override GUIContent toolbarIcon => EditModeButton;
        private GUIStyle ToggleStyle => m_toggleStyle ??= new GUIStyle(EditorStyles.toggle);

        private bool m_IsActive;
        private GUIStyle m_toggleStyle;

        LegacyPreferencesToolbar m_LegacyPreferencesToolbar = new LegacyPreferencesToolbar();

        public override void OnActivated()
        {
            if (this == null)
                return;

            m_IsActive = true;
            ColliderSelector.instance.Activate();
            ColliderCommands.instance.Activate();
            ColliderTransformToolManager.instance.Activate();
            ColliderDragAndDrop.instance.Activate();

            m_LegacyPreferencesToolbar.OnActivated();
        }

        public override void OnWillBeDeactivated()
        {
            if (this == null)
                return;

            m_LegacyPreferencesToolbar.OnDeactivated();

            ColliderDragAndDrop.instance.Deactivate();
            ColliderTransformToolManager.instance.Deactivate();
            ColliderSelector.instance.Deactivate();
            ColliderCommands.instance.Deactivate();
            m_IsActive = false;

        }

        void OnEnable()
        {
            if (this == null)
                return;

            ColliderSelector.colliderSelected += onColliderSelected;
        }

        void OnDisable()
        {
            if (this == null)
                return;

            ColliderSelector.colliderSelected -= onColliderSelected;
        }

        private void onColliderSelected()
        {
            if (m_IsActive || ToolManager.activeToolType == typeof(ColliderEditorTransformTools))
                return;

            ToolManager.SetActiveTool<ColliderEditorTransformTools>();
        }


        public override void OnToolGUI(EditorWindow window)
        {
            if(window is SceneView sceneView)
                m_LegacyPreferencesToolbar?.DrawToolbar(sceneView);

        }

    }
}
