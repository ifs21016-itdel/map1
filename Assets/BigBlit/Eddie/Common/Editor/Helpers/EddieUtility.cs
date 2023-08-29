using UnityEngine;
using UnityEditor;

using System.Reflection;
using System;

namespace BigBlit.Eddie
{
    public static class EddieUtility
    {
        public static string s_RootPath = "BigBlit/Eddie";

        public static string GetEddieRootDirectory()
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(0, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(0);
            string path = stackFrame.GetFileName();

            path = path.Replace('\\', '/');
            int index = path.IndexOf(s_RootPath);
            if (index == -1)
            {
                Debug.LogError("Eddie root folder must be parented to BigBlit folder.");
                return "";
            }
            path = path.Substring(0, index);
            return "Assets/" + path.Substring(Application.dataPath.Length + 1) + s_RootPath;
        }

        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(GetEddieRootDirectory() + "/" + path);


        internal static void RegisterPrivateEvent<T,D>(T caster, string eventName, D eventFunc) where D : Delegate
        {
            Type casterType = typeof(T);
            var bindingFlags = BindingFlags.NonPublic | (caster == null ? BindingFlags.Static : BindingFlags.Instance);
            var eventInfo = casterType.GetEvent(eventName, bindingFlags);
            if (eventInfo == null)
            {
                Debug.LogError("RegisterPrivateEvent: Cannot get eventEventInfo for private event " + eventName + ". Please contact support. Unity Version:" + Application.unityVersion);
                return;
            }
            var addMethod = eventInfo.GetAddMethod(true);
            if (addMethod == null)
            {
                Debug.LogError("RegisterPrivateEvent: Cannot get AddMethod for private event " + eventName + ". Please contact support. Unity Version:" + Application.unityVersion);
                return;
            } 

            addMethod.Invoke(null, new object[] { eventFunc });
        }

            internal static void RemovePrivateEvent<T, D>(T caster, string eventName, D eventFunc)
        {
            Type casterType = typeof(T);
            var bindingFlags = BindingFlags.NonPublic | (caster == null ? BindingFlags.Static : BindingFlags.Instance);
            var eventInfo = casterType.GetEvent(eventName, bindingFlags);
            if (eventInfo == null)
            {
                Debug.LogError("RemovePrivateEvent: Cannot get eventEventInfo for private event " + eventName + ". Please contact support. Unity Version:" + Application.unityVersion);
                return;
            }

            var remMethod = eventInfo.GetRemoveMethod(true);
            if (remMethod == null)
            {
                Debug.LogError("RemovePrivateEvent: Cannot get RemoveMethod/AddMethod for private event " + eventName + ". Please contact support. Unity Version:" + Application.unityVersion);
                return;
            }

            remMethod.Invoke(null, new object[] { eventFunc });

        }

    }

}