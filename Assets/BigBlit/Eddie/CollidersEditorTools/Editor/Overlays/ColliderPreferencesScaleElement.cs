#if UNITY_2021_2_OR_NEWER

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{

    [EditorToolbarElement(ColliderPreferencesScaleElement.k_Id, new System.Type[] { typeof(SceneView) })]
    internal class ColliderPreferencesScaleElement : ColliderTransformToggleBase
    {
        public const string k_Id = "BigBlit/Eddie/Colliders/ScaleTool";

        protected override string Name => "Scale Colliders Tool";
        protected override string Tooltip => "Scale Colliders Tool";
        protected override Texture2D Icon => (Texture2D)EditorGUIUtility.IconContent("ScaleTool").image;
        protected override Type ToolType => typeof(ColliderTransformScale);

    }
}

#endif