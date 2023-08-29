#if UNITY_2021_2_OR_NEWER

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{


    [EditorToolbarElement(ColliderPreferencesEditBoundsElement.k_Id, new System.Type[] { typeof(SceneView) })]
    internal class ColliderPreferencesEditBoundsElement : ColliderTransformToggleBase
    {
        public const string k_Id = "BigBlit/Eddie/Colliders/EditBounds";

        protected override string Name => "Edit Collider Bounds Tool";
        protected override string Tooltip => "Edit Collider Bounds Tool";
        protected override Texture2D Icon => EditorGUIUtility.isProSkin ? ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/d_MidpointIcon.png") :
                 ColliderUtility.LoadAssetAtPath<Texture2D>("Editor/Resources/l_MidpointIcon.png");
        protected override Type ToolType => typeof(ColliderTransformBounds);
    }
}

#endif
