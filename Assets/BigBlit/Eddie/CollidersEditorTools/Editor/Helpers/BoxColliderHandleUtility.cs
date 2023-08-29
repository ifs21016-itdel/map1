using UnityEngine;
using UnityEditor;
using System;

namespace BigBlit.Eddie.CollidersEditorTools
{
    public static class BoxColliderHandleUtility
    {

        private static bool ColorCodedCenterlines => true;

        internal static void DrawWireCube(Vector3 center, Vector3 size, float thickness)
        {
            Vector3 vector3 = size * 0.5f;
            Vector3[] vector3Array = new Vector3[8]
            {
                center + new Vector3(-vector3.x, -vector3.y, -vector3.z),
                center + new Vector3(-vector3.x, vector3.y, -vector3.z),
                center + new Vector3(vector3.x, vector3.y, -vector3.z),
                center + new Vector3(vector3.x, -vector3.y, -vector3.z),

                center + new Vector3(-vector3.x, -vector3.y, vector3.z),
                center + new Vector3(-vector3.x, vector3.y, vector3.z),
                center + new Vector3(vector3.x, vector3.y, vector3.z),
                center + new Vector3(vector3.x, -vector3.y, vector3.z),

            };

            Handles.DrawLine(vector3Array[0], vector3Array[1], thickness);
            Handles.DrawLine(vector3Array[1], vector3Array[2], thickness);
            Handles.DrawLine(vector3Array[2], vector3Array[3], thickness);
            Handles.DrawLine(vector3Array[3], vector3Array[0], thickness);

            Handles.DrawLine(vector3Array[4], vector3Array[5], thickness);
            Handles.DrawLine(vector3Array[5], vector3Array[6], thickness);
            Handles.DrawLine(vector3Array[6], vector3Array[7], thickness);
            Handles.DrawLine(vector3Array[7], vector3Array[4], thickness);

            Handles.DrawLine(vector3Array[0], vector3Array[4], thickness);
            Handles.DrawLine(vector3Array[1], vector3Array[5], thickness);
            Handles.DrawLine(vector3Array[2], vector3Array[6], thickness);
            Handles.DrawLine(vector3Array[3], vector3Array[7], thickness);
        }

        internal static void SetHandleDeltaCenter(BoxCollider boxCollider, Vector3 deltaPos)
        {
            boxCollider.center += (Vector3)(boxCollider.transform.localToWorldMatrix.inverse * (Handles.matrix * deltaPos));
        }

        internal static void DrawBoxColliderWireFrame(BoxColliderProxy boxCollider, bool showCenterlines, bool colorCoded, float thickness)
        {
            Bounds bounds = boxCollider.HandleBounds;

            if (Mathf.Approximately(Handles.color.a, 0.0f))
            {
                if (showCenterlines)
                    ColliderHandleUtility.DrawCenterLines(bounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);
                return;
            }

            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            var prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.GreaterEqual;
            Color prevColor = Handles.color;
            Handles.color = new Color(prevColor.r * 0.75f, prevColor.g * 0.75f, prevColor.b * 0.75f, prevColor.a * 0.35f);
            DrawWireCube(center, size, thickness * 0.75f);
            Handles.color = prevColor;

            if (showCenterlines)
                ColliderHandleUtility.DrawCenterLines(bounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
            DrawWireCube(center, size, thickness);
            Handles.zTest = prevZTest;
            Handles.color = prevColor;
        }

    }
}
