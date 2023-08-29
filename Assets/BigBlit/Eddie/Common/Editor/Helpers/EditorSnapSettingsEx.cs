using UnityEditor;
using System.Reflection;

namespace BigBlit.Eddie
{
    public static class EditorSnapSettingsEx
    {
#if UNITY_2022_2_OR_NEWER
        public static bool IncrementalSnapActive => EditorSnapSettings.incrementalSnapActive;
        public static bool GridSnapActive => EditorSnapSettings.gridSnapActive;
#else
        public static bool IncrementalSnapActive => (bool)typeof(EditorSnapSettings).GetProperty("incrementalSnapActive", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        public static bool GridSnapActive => (bool)typeof(EditorSnapSettings).GetProperty("gridSnapActive", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
#endif
        public static bool VertexSnapActive => (bool)typeof(EditorSnapSettings).GetProperty("vertexSnapActive", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
    }
}