#if UNITY_2021_2_OR_NEWER

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{

    [EditorToolbarElement(ColliderPreferencesRotateElement.k_Id, new System.Type[] { typeof(SceneView) })]
    internal class ColliderPreferencesRotateElement : ColliderTransformToggleBase
    {
        public const string k_Id = "BigBlit/Eddie/Colliders/RotateTool";

        protected override string Name => "Rotate Colliders Tool";
        protected override string Tooltip => "Rotate Colliders Tool";
        protected override Texture2D Icon => (Texture2D)EditorGUIUtility.IconContent("RotateTool").image;
        protected override Type ToolType => typeof(ColliderTransformRotate);
    }
}

#endif
