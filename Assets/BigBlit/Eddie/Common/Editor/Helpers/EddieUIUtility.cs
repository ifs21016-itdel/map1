using UnityEditor;
using UnityEngine;

namespace BigBlit.Eddie
{
    internal class EddieUIUtility
    {
        public static bool WorldClosestPointOnLine(Vector3 origin, Vector3 direction, out Vector3 worldPoint)
        {
            CalcParamOnConstraint(SceneView.currentDrawingSceneView.camera, Event.current.mousePosition, origin, direction, out var len);
            worldPoint = origin + direction * len;
            return true;
        }
       
        internal static float GetPerspectiveCameraDistance(float objectSize, float fov) => objectSize / Mathf.Sin((float)((double)fov * 0.5 * (Mathf.PI / 180.0)));

        internal static Vector3 ScreenToWorldDistance(SceneView sceneView, Vector2 delta)
        {
            Camera camera = sceneView.camera;
            float nearClipPlane = camera.nearClipPlane;
            float farClipPlane = camera.farClipPlane;
            Vector3 position1 = camera.transform.position;
            float size = sceneView.size;
            sceneView.size = Mathf.Min(sceneView.size, 2.5E+07f);
            float num = size / sceneView.size;
            Vector2 dynamicClipPlanes = GetDynamicClipPlanes(sceneView);
            sceneView.camera.nearClipPlane = dynamicClipPlanes.x;
            sceneView.camera.farClipPlane = dynamicClipPlanes.y;
            sceneView.camera.transform.position = Vector3.zero;
            Vector3 position2 = camera.transform.rotation * new Vector3(0.0f, 0.0f, sceneView.cameraDistance);
            Vector3 position3 = camera.WorldToScreenPoint(position2) + new Vector3(delta.x, delta.y, 0.0f);
            Vector3 worldDistance = (camera.ScreenToWorldPoint(position3) - position2) * (EditorGUIUtility.pixelsPerPoint * num);
            sceneView.size = size;
            sceneView.camera.nearClipPlane = nearClipPlane;
            sceneView.camera.farClipPlane = farClipPlane;
            sceneView.camera.transform.position = position1;
            return worldDistance;
        }

        internal static Vector2 GetDynamicClipPlanes(SceneView sceneView)
        {
            float y = Mathf.Clamp(2000f * sceneView.size, 1000f, 1.844674E+19f);
            return new Vector2(y * 5E-06f, y);
        }

        internal static Vector3[] GetBoundsRotated(Bounds bounds, Quaternion rotation)
        {
            return GetRectRotated(bounds.center, bounds.size, rotation);
        }

        internal static Vector3[] GetRectRotated(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Vector3 e = size / 2.0f;
            return new Vector3[] {
         center + rotation * new Vector3(e.x, -e.y, -e.z),
        center + rotation * new Vector3(e.x, e.y, -e.z),
        center + rotation * new Vector3(e.x, e.y, e.z),
        center + rotation * new Vector3(e.x, -e.y, e.z),

        center + rotation * new Vector3(-e.x, -e.y, -e.z),
        center + rotation * new Vector3(-e.x, e.y, -e.z),
        center + rotation * new Vector3(-e.x, e.y, e.z),
        center + rotation * new Vector3(-e.x, -e.y, e.z)};
        }

        public static Vector3 GetAlignedDirection(Vector3[] directions, Vector3 axis)
        {
            float maxDot = Mathf.NegativeInfinity;
            Vector3 dir = Vector3.zero;

            for (int i = 0; i < 6; i++)
            {
                float dot = Vector3.Dot(directions[i], axis);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    dir = directions[i];
                }
            }

            return dir;
        }

        public static Vector3 GetLastAlignedDirection(Vector3[] directions, Vector3 axis)
        {
            float maxDot = Mathf.Infinity;
            Vector3 dir = Vector3.zero;

            for (int i = 0; i < 6; i++)
            {
                float dot = Mathf.Abs(Vector3.Dot(directions[i], axis));
                if (dot < maxDot)
                {
                    maxDot = dot;
                    dir = directions[i];
                }
            }

            return dir;
        }

        public static Vector3[] CreateObjectDirections(GameObject go)
        {
            var t = go.transform;
            return new Vector3[] { -t.right, -t.up, -t.forward, t.right, t.up, t.forward };
        }

        public static bool CalcParamOnConstraint(
        Camera camera,
        Vector2 guiPosition,
        Vector3 constraintOrigin,
        Vector3 constraintDir,
        out float parameterization)
        {
            Vector3 rhs = Vector3.Cross(constraintDir, camera.transform.position - constraintOrigin);
            Plane plane = new Plane(Vector3.Cross(constraintDir, rhs), constraintOrigin);
            Ray worldRay = HandleUtility.GUIPointToWorldRay(guiPosition);
            float enter;
            if ((double)Vector3.Dot(worldRay.direction, plane.normal) > 0.0500000007450581 && plane.Raycast(worldRay, out enter))
            {
                Vector3 point = worldRay.GetPoint(enter);
                parameterization = HandleUtility.PointOnLineParameter(point, constraintOrigin, constraintDir);
                return !float.IsInfinity(parameterization);
            }
            parameterization = 0.0f;
            return false;
        }

        public static float DistanceToInfiniteLine(Vector3 origin, Vector3 direction)
        {
            var p0 = HandleUtility.WorldToGUIPoint(origin);
            var p1 = HandleUtility.WorldToGUIPoint(origin + direction);
            return (FindNearestPointOnLine(p0, p1 - p0, Event.current.mousePosition) - Event.current.mousePosition).magnitude;
        }

        public static Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 direction, Vector2 point)
        {
            direction.Normalize();
            Vector2 lhs = point - origin;

            float dotP = Vector2.Dot(lhs, direction);
            return origin + direction * dotP;
        }



    }
}
