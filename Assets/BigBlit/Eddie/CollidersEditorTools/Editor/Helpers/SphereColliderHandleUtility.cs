using UnityEditor;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal static class SphereColliderHandleUtility
    {
        private static bool ColorCodedCenterlines => true;

        public static void SetCenterFromHandleDeltaPos(SphereCollider sphereCollider, Vector3 deltaPos)
        {
            sphereCollider.center += sphereCollider.center += sphereCollider.center += (Vector3)(sphereCollider.transform.localToWorldMatrix.inverse * (Handles.matrix * deltaPos));
        }

        public static Vector3 GetHandleSpaceCenter(SphereCollider sphereCollider)
        {
            return Handles.inverseMatrix * (sphereCollider.transform.localToWorldMatrix * sphereCollider.center);
        }

        public static void DrawSphereWireFrame(SphereColliderProxy sphereCollider, bool showCenterlines, bool colorCoded, float thickness)
        {
            var handleBounds = sphereCollider.HandleBounds;

            if (Mathf.Approximately(Handles.color.a, 0.0f))
            {
                if (showCenterlines)
                    ColliderHandleUtility.DrawCenterLines(handleBounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);
                return;
            }

            Vector3 center = handleBounds.center;
            float radius = handleBounds.size.x * 0.5f;

            if (showCenterlines)
                ColliderHandleUtility.DrawCenterLines(handleBounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);

            Color prevColor = Handles.color;
            var prevZTest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.GreaterEqual;
            var insideThickness = thickness * 0.75f;
            Handles.color = new Color(prevColor.r * 0.75f, prevColor.g * 0.75f, prevColor.b * 0.75f, prevColor.a * 0.35f);
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius, insideThickness);
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360f, radius, insideThickness);
            Handles.DrawWireArc(center, Vector3.right, Vector3.forward, 360f, radius, insideThickness);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
            Handles.color = prevColor;
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius, thickness);
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360f, radius, thickness);
            Handles.DrawWireArc(center, Vector3.right, Vector3.forward, 360f, radius, thickness);
            Handles.color = prevColor;
            Handles.zTest = prevZTest;
        }
    }
}
