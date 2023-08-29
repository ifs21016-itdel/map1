using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace BigBlit.Eddie
{
    public static class HandlesEx
    {
        public struct PositionHandleIds
        {
            public readonly object idsObject;

            public readonly int x;
            public readonly int y;
            public readonly int z;
            public readonly int xyz;

#if UNITY_2022_2_OR_NEWER
            public static PositionHandleIds @default => new PositionHandleIds(Handles.PositionHandleIds.@default);


            PositionHandleIds(object idsObject)
            {

                this.idsObject = idsObject;
                var ids = (Handles.PositionHandleIds) idsObject;
                x = ids.x;
                y = ids.y;
                z = ids.z;
                xyz = ids.xyz;
            }
#else
            PositionHandleIds(object idsObject)
            {
          
                var idsType = typeof(Handles).GetNestedType("PositionHandleIds", BindingFlags.NonPublic);
                this.idsObject = idsObject;
                x = GetValue(idsObject, idsType, "x");
                y = GetValue(idsObject, idsType, "y");
                z = GetValue(idsObject, idsType, "z");
                xyz = GetValue(idsObject, idsType, "xyz");
            }
            
            public static PositionHandleIds @default
            {
                get
                {
                    var idsType = typeof(Handles).GetNestedType("PositionHandleIds", BindingFlags.NonPublic);
                    var defaultType = idsType.GetProperty("default", BindingFlags.Static | BindingFlags.Public);
                    var ids = defaultType.GetValue(null);
                    return new PositionHandleIds(ids);

                }
            }

            static int GetValue(object ids, Type idsType, string fieldName)
            {
                return (int)idsType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public).GetValue(ids);
            }
#endif
        }

        public static Vector3 DoPositionHandle_Internal(PositionHandleIds positionHandleIds, Vector3 position, Quaternion rotation, object positionHandleParams)
        {
            return (Vector3)typeof(Handles).GetMethod("DoPositionHandle_Internal", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { positionHandleIds.idsObject, position, rotation, positionHandleParams });
        }

        public static object DefaultPositionHandle => typeof(Handles).GetNestedType("PositionHandleParam", BindingFlags.NonPublic)
        .GetField("DefaultHandle", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        public static object DefaultFreePositionHandle => typeof(Handles).GetNestedType("PositionHandleParam", BindingFlags.NonPublic)
        .GetField("DefaultFreeMoveHandle", BindingFlags.Public | BindingFlags.Static).GetValue(null);




    }


}