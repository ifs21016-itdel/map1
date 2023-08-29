using UnityEditor;
using System.Reflection;

namespace BigBlit.Eddie
{
    public static class SceneViewEx
    {
        public static bool GetViewIsLockedToObject(this SceneView sceneView) =>
         (bool) typeof(SceneView).GetProperty("viewIsLockedToObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sceneView);
                  
        public static void SetViewIsLockedToObject(this SceneView sceneView, bool isLocked) =>
         typeof(SceneView).GetProperty("viewIsLockedToObject", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sceneView, isLocked);
           
    }
}