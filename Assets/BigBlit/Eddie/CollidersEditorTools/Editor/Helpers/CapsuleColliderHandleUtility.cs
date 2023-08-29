using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal static class CapsuleColliderHandleUtility
    {
        public static void DrawCapsuleWireFrame(CapsuleColliderProxy capsuleCollider, bool showCenterlines, bool colorCoded, float thickness)
        {

            Bounds handleBounds = capsuleCollider.HandleBounds;
            if (Mathf.Approximately(Handles.color.a, 0.0f))
            {
                if (showCenterlines)
                    ColliderHandleUtility.DrawCenterLines(handleBounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);
                return;
            }

            Vector3 right = capsuleCollider.Right;
            Vector3 up = capsuleCollider.Up;
            Vector3 forward = capsuleCollider.Forward;

            float handleRadius = capsuleCollider.GetHandleRadiusAndHeight(out var handleHeight);
            Vector3 vec3ScaledHeight = up * handleHeight;
            Vector3 handleCenter = handleBounds.center;
            var halfHeightVec = vec3ScaledHeight * 0.5f;
            var downPoint = handleCenter - halfHeightVec;
            var upPoint = handleCenter + halfHeightVec;
            var downSphereCenter = downPoint + up * handleRadius;
            var upSphereCenter = upPoint - up * handleRadius;

            if (showCenterlines)
                ColliderHandleUtility.DrawCenterLines(handleBounds, thickness, colorCoded, UnityEngine.Rendering.CompareFunction.Always);

            var insideThickness = thickness * 0.75f;
            var prevZTest = Handles.zTest;
            var prevColor = Handles.color;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.GreaterEqual;
            Handles.color = new Color(prevColor.r * 0.75f, prevColor.g * 0.75f, prevColor.b * 0.75f, prevColor.a * 0.35f);

            Handles.DrawWireArc(downSphereCenter, up, right, 360.0f, handleRadius, insideThickness);
            Handles.DrawWireArc(upSphereCenter, up, right, 360.0f, handleRadius, insideThickness);

            Handles.DrawWireArc(downSphereCenter, right, forward, 180.0f, handleRadius, insideThickness);
            Handles.DrawWireArc(downSphereCenter, forward, -right, 180.0f, handleRadius, insideThickness);

            Handles.DrawWireArc(upSphereCenter, right, -forward, 180.0f, handleRadius, insideThickness);
            Handles.DrawWireArc(upSphereCenter, forward, right, 180.0f, handleRadius, insideThickness);

            Handles.DrawLine(upSphereCenter + right * handleRadius, downSphereCenter + right * handleRadius, insideThickness);
            Handles.DrawLine(upSphereCenter - right * handleRadius, downSphereCenter - right * handleRadius, insideThickness);
            Handles.DrawLine(upSphereCenter + forward * handleRadius, downSphereCenter + forward * handleRadius, insideThickness);
            Handles.DrawLine(upSphereCenter - forward * handleRadius, downSphereCenter - forward * handleRadius, insideThickness);

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
            Handles.color = prevColor;

            Handles.DrawWireArc(downSphereCenter, up, right, 360.0f, handleRadius,thickness);
            Handles.DrawWireArc(upSphereCenter, up, right, 360.0f, handleRadius, thickness);

            Handles.DrawWireArc(downSphereCenter, right, forward, 180.0f, handleRadius, thickness);
            Handles.DrawWireArc(downSphereCenter, forward, -right, 180.0f, handleRadius, thickness);

            Handles.DrawWireArc(upSphereCenter, right, -forward, 180.0f, handleRadius, thickness);
            Handles.DrawWireArc(upSphereCenter, forward, right, 180.0f, handleRadius, thickness);

            Handles.DrawLine(upSphereCenter + right * handleRadius, downSphereCenter + right * handleRadius, thickness);
            Handles.DrawLine(upSphereCenter - right * handleRadius, downSphereCenter - right * handleRadius, thickness);
            Handles.DrawLine(upSphereCenter + forward * handleRadius, downSphereCenter + forward * handleRadius, thickness);
            Handles.DrawLine(upSphereCenter - forward * handleRadius, downSphereCenter - forward * handleRadius, thickness);

            Handles.zTest = prevZTest;
        }
    }
}