using UnityEngine;
using UnityEditor;
using System;

namespace BigBlit.Eddie.CollidersEditorTools
{

    public class LegacyPreferencesToolbar
    {
        public enum ToolbarLocation { Off, BottomCenter, BottomLeft, BottomRight, UpperLeft, UpperRight, UpperCenter };

        public static GUIStyle m_commandStyle;
        public static GUIContent[] m_toolsIcons;

        public static GUIStyle CommandStyle => m_commandStyle != null ? m_commandStyle : m_commandStyle = "command";
        public static GUIContent[] ToolsIcons => m_toolsIcons != null ? m_toolsIcons : m_toolsIcons = loadToolsIcons();
        public static Type[] m_toolsTypes = new Type[] {
        typeof(ColliderTransformMove),
        typeof(ColliderTransformRotate),
        typeof(ColliderTransformScale),
        typeof(ColliderTransformBounds),
        typeof(ColliderTransformNone)
        };

        private static ToolbarLocation ToolbarLocationPref => (ToolbarLocation)ColliderPreferences.instance.LegacyToolbar.Value;

        public void OnActivated()
        {

        }

        public void OnDeactivated()
        {

        }

        public void DrawToolbar(SceneView sceneView)
        {
            if (ToolbarLocationPref == ToolbarLocation.Off)
                return;

            int currentToolId = getActiveToolId();

            EditorGUI.BeginChangeCheck();

            Handles.BeginGUI();
            var srgbWrite = GL.sRGBWrite;
            GL.sRGBWrite = false;
            int selectedToolId = GUI.Toolbar(calculateToolbarRect(sceneView), currentToolId, ToolsIcons, CommandStyle);
            GL.sRGBWrite = srgbWrite;
            Handles.EndGUI();

            if (EditorGUI.EndChangeCheck())
            {
                if (selectedToolId == currentToolId)
                    selectedToolId = 4;
                setActiveToolById(selectedToolId);
            }
        }

        private Rect calculateToolbarRect(SceneView sceneView)
        {

            int screenWidth = (int)sceneView.position.width;
            int screenHeight = (int)sceneView.position.height;
            var toolbarRect = new Rect(10, 10, 128, 32);

            switch (ToolbarLocationPref)
            {
                case ToolbarLocation.BottomCenter:
                    toolbarRect.x = (screenWidth / 2 - 64);
                    toolbarRect.y = screenHeight - toolbarRect.height * 2;
                    break;

                case ToolbarLocation.BottomLeft:
                    toolbarRect.x = 12;
                    toolbarRect.y = screenHeight - toolbarRect.height * 2;
                    break;

                case ToolbarLocation.BottomRight:
                    toolbarRect.x = screenWidth - (toolbarRect.width + 12);
                    toolbarRect.y = screenHeight - toolbarRect.height * 2;
                    break;

                case ToolbarLocation.UpperLeft:
#if UNITY_2021_1_OR_NEWER
                    toolbarRect.x = 64;
#else
    toolbarRect.x = 12;
#endif
                    toolbarRect.y = 10;
                    break;

                case ToolbarLocation.UpperRight:
                    toolbarRect.x = screenWidth - (toolbarRect.width + 96);
                    toolbarRect.y = 10;
                    break;

                default:
                case ToolbarLocation.UpperCenter:
                    toolbarRect.x = (screenWidth / 2 - 64);
                    toolbarRect.y = 10;
                    break;
            }

            return toolbarRect;

        }


        private void setActiveToolById(int id) => ColliderTransformToolManager.instance.SetActiveTool(m_toolsTypes[id]);

        private int getActiveToolId()
        {
            var type = ColliderTransformToolManager.instance.ActiveToolType;

            for (int i = 0; i < m_toolsTypes.Length; i++)
            {
                if (m_toolsTypes[i] == type)
                    return i;
            }

            return 0;
        }

        private static GUIContent[] loadToolsIcons()
        {
            var moveContent = new GUIContent(EditorGUIUtility.IconContent("MoveTool")?.image, "Move Colliders Tool");
            var rotateContent = new GUIContent( EditorGUIUtility.IconContent("RotateTool")?.image, "Rotate Colliders Tool");
            var scaleContent =  new GUIContent(EditorGUIUtility.IconContent("ScaleTool")?.image, "Scale Colliders Tool");

            var boundsIcon = EditorGUIUtility.isProSkin ? ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/d_MidpointIcon.png") :
                 ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/l_MidpointIcon.png");
            var boundsContent = new GUIContent(boundsIcon, "Edit Collider Bounds Tool");

            return new GUIContent[] {
           moveContent != null ? moveContent : new GUIContent("M", "Move"),
           rotateContent != null ? rotateContent: new GUIContent("R", "Rotate"),
           scaleContent != null ? scaleContent : new GUIContent("S", "Scale"),
           boundsContent != null ? boundsContent : new GUIContent("B", "Bounds"),
        };
        }
    }
}