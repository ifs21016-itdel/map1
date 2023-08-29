using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BigBlit.Eddie
{
    public static class EditorStylesEx
    {
#if UNITY_2021_2_OR_NEWER
        public static GUIStyle SelectionRect => EditorStyles.selectionRect;
#else
        public static GUIStyle SelectionRect => (GUIStyle)typeof(EditorStyles).GetProperty("selectionRect", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
#endif
    }
}