#if UNITY_2021_2_OR_NEWER

using UnityEditor;
using UnityEditor.Toolbars;

namespace BigBlit.Eddie
{
    [EditorToolbarElement(ToolbarSpacerElement.k_Id, new System.Type[] { typeof(SceneView) })]
    public class ToolbarSpacerElement : UnityEditor.UIElements.ToolbarSpacer
    {
        public const string k_Id = "BigBlit/Eddie/ToolbarSpacerElement";
    }
}

#endif
