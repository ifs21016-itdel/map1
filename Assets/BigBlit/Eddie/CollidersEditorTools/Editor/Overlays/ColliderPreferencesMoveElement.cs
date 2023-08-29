#if UNITY_2021_2_OR_NEWER

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{



    [EditorToolbarElement(ColliderPreferencesMoveElement.k_Id, new System.Type[] { typeof(SceneView) })]
    internal class ColliderPreferencesMoveElement : ColliderTransformToggleBase
    {
        public const string k_Id = "BigBlit/Eddie/Colliders/MoveTool";

        protected override string Name => "Move Colliders Tool";
        protected override string Tooltip => "Move Colliders Tool";
        protected override Texture2D Icon => (Texture2D)EditorGUIUtility.IconContent("MoveTool").image;
        protected override Type ToolType => typeof(ColliderTransformMove);
    }
}

#endif

