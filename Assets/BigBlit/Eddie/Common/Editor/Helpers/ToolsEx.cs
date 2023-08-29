using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BigBlit.Eddie
{
    public static class ToolsEx
    {
        public static Quaternion HandleLocalRotation => (Quaternion)typeof(Tools).GetProperty("handleLocalRotation", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        public static bool VertexDragging 
        {
            get => (bool) typeof(Tools).GetField("vertexDragging", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            set =>  typeof(Tools).GetField("vertexDragging", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }
    }
}