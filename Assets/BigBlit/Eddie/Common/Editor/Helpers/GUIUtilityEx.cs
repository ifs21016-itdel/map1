using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace BigBlit.Eddie
{
    public static class GUIUtilityEx
    {
        public static int GetPermanentControlID() => (int) typeof(GUIUtility).GetMethod("GetPermanentControlID", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
    }
}