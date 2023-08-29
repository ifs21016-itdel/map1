using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal static class ColliderHandleUtility
    {
        internal static float Vec3ToFloatMax(Vector3 vector3) => Mathf.Max(vector3.x, vector3.y, vector3.z);

        internal static Vector3 GetLossyScaleAbs(Collider collider)
        {
            var lossyScale = collider.transform.lossyScale;
            return new Vector3(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
        }

        public static Vector3 GetLossyScaleInverted(Collider collider)
        {
            var lossyScale = collider.transform.lossyScale;
            return new Vector3(1.0f / lossyScale.x, 1.0f / lossyScale.y, 1.0f / lossyScale.z);
        }

        public static Vector3 GetLossyScaleInvertedAbs(Collider collider)
        {
            var lossyScale = collider.transform.lossyScale;
            return new Vector3(1.0f / Mathf.Abs(lossyScale.x), 1.0f / Mathf.Abs(lossyScale.y), 1.0f / Mathf.Abs(lossyScale.z));
        }

        public static Quaternion GetHandleRotation(ICollider collider)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                return Quaternion.identity;
            else
                return collider != null ? collider.Transform.rotation : Quaternion.identity;
        }

        public static Quaternion GetHandleRotation()
        {
            return GetHandleRotation(ColliderSelection.Collider);
        }

        public static Vector3 GetHandlePosition(ICollider[] colliders, ICollider collider)
        {
            return Tools.pivotMode == PivotMode.Center ? GetWorldCentroid(colliders) : GetPivot(collider);

        }

        public static Vector3 GetHandlePosition()
        {
            var colliders = ColliderSelection.Colliders;
            if (colliders.Length == 0)
                return Tools.handlePosition;

            var collider = colliders.First();
            return GetHandlePosition(colliders, collider);
        }

        public static Vector3 GetPivot(ICollider collider) => collider.WorldBounds.center;

        public static Matrix4x4 GetHandlesMatrix(Collider collider)
        {
            return Matrix4x4.TRS(collider.transform.position, collider.transform.rotation, Vector3.one);
        }

        public static Rect ToRect(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if ((double)rect.width < 0.0)
            {
                rect.x += rect.width;
                rect.width = -rect.width;
            }
            if ((double)rect.height < 0.0)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        public static ICollider GetClosestColliderAtGUIPos(Vector2 guiPosition, ICollider[] targets, out Vector3 hitPoint)
        {
            return GetClosestColliderAtWorldRay(HandleUtility.GUIPointToWorldRay(guiPosition), targets, out hitPoint);
        }

        internal static ICollider GetNextColliderAtGUIPos(Vector2 guiPosition, ICollider currentCollider, ICollider[] targets, out Vector3 hitPoint)
        {
            return GetNextColliderAtWorldRay(HandleUtility.GUIPointToWorldRay(guiPosition), currentCollider, targets, out hitPoint);
        }

        internal static ICollider GetNextColliderAtWorldRay(Ray worldRay, ICollider currentCollider, ICollider[] targets, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;

            if (currentCollider == null || !GetColliderDistanceAtWorldRay(currentCollider, worldRay, out var prevColDist))
                return GetClosestColliderAtWorldRay(worldRay, targets, out hitPoint);

            float nextMinDist = Mathf.Infinity;
            float minDist = Mathf.Infinity;

            ICollider nextMinCol = null;
            ICollider minCol = null;

            foreach (var target in targets)
            {
                if (target == currentCollider)
                    continue;

                if (!GetColliderDistanceAtWorldRay(target, worldRay, out var targetDist))
                    continue;

                if (targetDist < minDist)
                {
                    minDist = targetDist;
                    minCol = target;
                }

                if (targetDist <= prevColDist)
                    continue;

                if (targetDist < nextMinDist)
                {

                    nextMinDist = targetDist;
                    nextMinCol = target;
                }
            }

            return nextMinCol ?? minCol ?? currentCollider;
        }

        internal static bool GetColliderDistanceAtWorldRay(ICollider collider, Ray worldRay, out float distance)
        {
            distance = 0;
            if (!collider.Target.Raycast(worldRay, out var hitInfo, 10000000.0f))
                return false;

            distance = hitInfo.distance;
            return true;
        }

        public static ICollider GetClosestColliderAtWorldRay(Ray worldRay, ICollider[] targets, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            ICollider minCollider = null;
            float minDist = Mathf.Infinity;

            if (targets.Length == 0)
                return null;

            foreach (var collider in targets)
            {
                if (!collider.IsTargetValid)
                    continue;

                if (collider.Target.Raycast(worldRay, out var hitInfo, Mathf.Infinity))
                {
                    if (minDist <= hitInfo.distance)
                        continue;
                    minDist = hitInfo.distance;
                    minCollider = collider;
                    hitPoint = hitInfo.point;
                }
            }

            return minCollider;
        }

        public static Vector3 GetWorldCentroid(ICollider[] colliders)
        {
            Bounds bounds = new Bounds(colliders[0].WorldBounds.center, Vector3.zero);
            foreach (var collider in colliders)
            {
                if (collider.IsTargetValid)
                    bounds.Encapsulate(collider.WorldBounds.center);
            }

            return bounds.center;
        }

        public static Vector3 GetMinDragDifference(Vector3 newHandlePos)
        {
            return Vector3.one * (HandleUtility.GetHandleSize(newHandlePos) / 80.0f);
        }

        public static bool FindNearestGOCenter(out float distance, out Vector3 vertexPos)
        {
            vertexPos = Vector3.zero;
            distance = 0.0f;
            Vector2 guiPos = Event.current.mousePosition;
            var go = HandleUtility.PickGameObject(guiPos, false);
            if (go == null)
                return false;

            Vector3 center = go.transform.position;
            var ray = HandleUtility.GUIPointToWorldRay(guiPos);
            distance = HandleUtility.DistancePointLine(center, ray.origin, ray.origin + ray.direction * 100000.0f);
            vertexPos = center;
            return true;
        }

        public static bool FindNearestDraggingPos(out Vector3 nearestPos)
        {
            bool result = false;
            float distance = Mathf.Infinity;
            nearestPos = Vector3.zero;
            if (HandleUtility.FindNearestVertex(Event.current.mousePosition, out var vertexPos))
            {
                result |= true;
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                distance = HandleUtility.DistancePointLine(vertexPos, ray.origin, ray.origin + ray.direction * 100000.0f);
                nearestPos = vertexPos;
            }

            if (FindNearestGOCenter(out var dist, out var pos))
            {
                result |= true;
                if (dist < distance)
                    nearestPos = pos;
            }
            return result;
        }

        public static Vector3 GetSnapped(HandlesEx.PositionHandleIds positionHandleIds, Vector3 position, Quaternion rotation)
        {
            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
                return position;


            if (!FindNearestDraggingPos(out var vertexPos))
                return position;

            if (GUIUtility.hotControl == positionHandleIds.xyz)
                return vertexPos;

            Vector3 delta = vertexPos - position;
            if (GUIUtility.hotControl == positionHandleIds.x)
                delta = Vector3.Project(delta, rotation * Vector3.right);
            if (GUIUtility.hotControl == positionHandleIds.y)
                delta = Vector3.Project(delta, rotation * Vector3.up);
            if (GUIUtility.hotControl == positionHandleIds.z)
                delta = Vector3.Project(delta, rotation * Vector3.forward);

            return position + delta;

        }

        public static Vector3 DoPositionHandle(HandlesEx.PositionHandleIds positionHandleIds, Vector3 position, Quaternion rotation)
        {
            var vertexDragging = ToolsEx.VertexDragging;
            ToolsEx.VertexDragging = false;
            var newHandlePos = HandlesEx.DoPositionHandle_Internal(positionHandleIds, position, rotation,
            vertexDragging ? HandlesEx.DefaultFreePositionHandle : HandlesEx.DefaultPositionHandle);
            if (vertexDragging)
                newHandlePos = GetSnapped(positionHandleIds, newHandlePos, rotation);
            ToolsEx.VertexDragging = vertexDragging;
            return newHandlePos;
        }

        public static Vector3 RoundVec3(Vector3 vec3, Vector3 minDragDifference, bool[] shouldRound)
        {

            vec3.x = shouldRound[0] ? MathUtilsEx.RoundBasedOnMinimumDifference(vec3.x, minDragDifference.x) : vec3.x;
            vec3.y = shouldRound[1] ? MathUtilsEx.RoundBasedOnMinimumDifference(vec3.y, minDragDifference.y) : vec3.y;
            vec3.z = shouldRound[2] ? MathUtilsEx.RoundBasedOnMinimumDifference(vec3.z, minDragDifference.z) : vec3.z;
            return vec3;
        }

        public static void RoundCenter(ICollider collider, Vector3 minDragDifference, bool[] shouldRound)
        {
            var lossyScale = collider.Transform.lossyScale;
            minDragDifference.x /= lossyScale.x;
            minDragDifference.y /= lossyScale.y;
            minDragDifference.z /= lossyScale.z;
            collider.Center = RoundVec3(collider.Center, minDragDifference, shouldRound);
        }

        public static void RoundSize(ICollider collider, Vector3 minDragDifference, bool[] shouldRound)
        {
            var lossyScale = collider.Transform.lossyScale;
            minDragDifference.x /= lossyScale.x;
            minDragDifference.y /= lossyScale.y;
            minDragDifference.z /= lossyScale.z;
            collider.Size = RoundVec3(collider.Size, minDragDifference, shouldRound);
        }

        public static Vector3 FloatToVec3(float value)
        {
            return new Vector3(value, value, value);
        }

        public static Bounds GetCollidersWorldBounds(ICollider[] colliders)
        {
            if (colliders.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            var bounds = new Bounds(colliders[0].WorldBounds.center, colliders[0].WorldBounds.size);
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].WorldBounds);
            return bounds;
        }

        public static void DrawColliderFrame(ICollider collider, bool showCenterlines, bool colorCoded, float thickness)
        {
            if (collider is BoxColliderProxy boxColliderProxy)
                BoxColliderHandleUtility.DrawBoxColliderWireFrame(boxColliderProxy, showCenterlines, colorCoded, thickness);
            else if (collider is SphereColliderProxy sphereCollider)
                SphereColliderHandleUtility.DrawSphereWireFrame(sphereCollider, showCenterlines, colorCoded, thickness);
            else if (collider is CapsuleColliderProxy capsuleCollider)
                CapsuleColliderHandleUtility.DrawCapsuleWireFrame(capsuleCollider, showCenterlines, colorCoded, thickness);
        }

        internal static Vector3 LocalPointToHandleSpace(Transform transform, Vector3 worldPoint) => Handles.inverseMatrix * (transform.localToWorldMatrix * worldPoint);

        internal static Vector3 HandlePointToLocalSpace(Transform transform, Vector3 handlePoint) => transform.worldToLocalMatrix * (Handles.matrix * handlePoint);

        internal static Vector3 GetPositive(Vector3 vector) => Vector3.Max(vector, new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon));

        internal static Vector3 Vec3Abs(Vector3 size) => new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));

        internal static void DrawCenterLines(Bounds bounds, float thickness, bool colorCoded, UnityEngine.Rendering.CompareFunction zTest)
        {
            var center = bounds.center;
            var size = bounds.size;
            float minSize = Mathf.Min(size.x, size.y, size.z);

            var xColor = colorCoded ? Handles.xAxisColor : Handles.color;
            var yColor = colorCoded ? Handles.yAxisColor : Handles.color;
            var zColor = colorCoded ? Handles.zAxisColor : Handles.color;
            Color prevColor = Handles.color;
            var prevZTest = Handles.zTest;
            Handles.zTest = zTest;

            Handles.color = xColor;
            Handles.DrawLine(center + Vector3.right * minSize * 0.1f, center + Vector3.left * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.right * size.x * 0.5f, center + Vector3.left * size.x * 0.5f, 2.0f);

            Handles.color = yColor;
            Handles.DrawLine(center + Vector3.up * minSize * 0.1f, center + Vector3.down * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.up * size.y * 0.5f, center + Vector3.down * size.y * 0.5f, 2.0f);

            Handles.color = zColor;
            Handles.DrawLine(center + Vector3.forward * minSize * 0.1f, center + Vector3.back * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.forward * size.z * 0.5f, center + Vector3.back * size.z * 0.5f, 2.0f);

            Handles.color = xColor;
            Handles.DrawLine(center + Vector3.right * minSize * 0.1f, center + Vector3.left * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.right * size.x * 0.5f, center + Vector3.left * size.x * 0.5f, 2.0f);

            Handles.color = yColor;
            Handles.DrawLine(center + Vector3.up * minSize * 0.1f, center + Vector3.down * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.up * size.y * 0.5f, center + Vector3.down * size.y * 0.5f, 2.0f);

            Handles.color = zColor;
            Handles.DrawLine(center + Vector3.forward * minSize * 0.1f, center + Vector3.back * minSize * 0.1f, thickness);
            Handles.DrawDottedLine(center + Vector3.forward * size.z * 0.5f, center + Vector3.back * size.z * 0.5f, 2.0f);

            Handles.color = prevColor;
            Handles.zTest = prevZTest;
        }

        internal static void DrawColliderFrame(ICollider collider, bool showCenterlines, float boundsThickness, object centerlinesColorCoded)
        {
            throw new NotImplementedException();
        }

        internal static bool IsLossyScaleValid(ICollider target)
        {
           var lossyScale = target.Transform.lossyScale;
           return !(Mathf.Approximately(lossyScale.x, 0.0f) || Mathf.Approximately(lossyScale.y, 0.0f) || Mathf.Approximately(lossyScale.y, 0.0f));
        }
    }
}