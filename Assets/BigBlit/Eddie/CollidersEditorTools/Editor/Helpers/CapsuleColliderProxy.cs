using System;
using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class CapsuleColliderProxy : ColliderProxy
    {
        private static Vector3[] s_LookupDir = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
        private static int[] s_RightDirId = new int[] { 2, 0, 1 };
        private static int[] s_ForwardDirId = new int[] { 1, 2, 0 };
        private static int[,] s_DirectionArray = new int[,]
            {
                { 1, 0, 2 },
                { 2, 1, 0 },
                { 0 ,2 ,1}
            };


        public Vector3 Right => s_LookupDir[s_RightDirId[CapsuleTarget.direction]];
        public Vector3 Up => s_LookupDir[CapsuleTarget.direction];
        public Vector3 Forward => s_LookupDir[s_ForwardDirId[CapsuleTarget.direction]];

        public override Vector3 Center
        {
            get => CapsuleTarget.center;
            set => CapsuleTarget.center = value;
        }

        public override Vector3 Size
        {
            get
            {
                var radsize = Mathf.Max(Mathf.Abs(CapsuleTarget.radius) * 2.0f, Mathf.Epsilon);
                Vector3 size = Vector3.one * radsize;
                size[CapsuleTarget.direction] = Mathf.Max(Math.Abs(CapsuleTarget.height), radsize);
                return size;
            }
            set
            {
                value = Vector3.Max(ColliderHandleUtility.Vec3Abs(value), Vector3.one * Mathf.Epsilon);
                int dir = CapsuleTarget.direction;
                var r0 = value[s_RightDirId[dir]];
                var r1 = value[s_ForwardDirId[dir]];
                var prs = Mathf.Abs(CapsuleTarget.radius) * 2.0f;
                var r = Mathf.Max((Mathf.Abs(r0 - prs) > Mathf.Abs(r1 - prs) ? r0 : r1), Mathf.Epsilon);
                CapsuleTarget.height = Mathf.Max(value[dir], r);
                CapsuleTarget.radius = r * 0.5f;
            }
        }

        public override Vector3 HandleSize
        {
            get
            {

                Vector3 lossyAbs = ColliderHandleUtility.GetLossyScaleAbs(m_Target);
                int dir = CapsuleTarget.direction;
                float rs = Mathf.Max(Mathf.Abs(CapsuleTarget.radius * 2.0f) * Mathf.Max(lossyAbs[s_RightDirId[dir]], lossyAbs[s_ForwardDirId[dir]]), Mathf.Epsilon);
                Vector3 size = Vector3.one * rs;
                size[dir] = Mathf.Max(Mathf.Abs(CapsuleTarget.height) * lossyAbs[dir], rs, Mathf.Epsilon);
                return size;
            }

            set
            {
                Vector3 lossyAbs = ColliderHandleUtility.GetLossyScaleAbs(m_Target);
                int dir = CapsuleTarget.direction;
                var rscale = Mathf.Max(lossyAbs[s_RightDirId[dir]], lossyAbs[s_ForwardDirId[dir]], Mathf.Epsilon);
                var hscale = lossyAbs[dir];

                float height = value[dir] / hscale;
                var prs = Mathf.Abs(CapsuleTarget.radius) * 2.0f;
                var r0 = value[s_RightDirId[dir]];
                var r1 = value[s_ForwardDirId[dir]];
                var r = Mathf.Max((Mathf.Abs(r0 - prs * rscale) > Mathf.Abs(r1 - prs * rscale) ? r0 : r1), Mathf.Epsilon);
               
                CapsuleTarget.radius = r / rscale *  0.5f;
                CapsuleTarget.height =  height; 
            }
        }

        public override Vector3 HandleCenter
        {
            get => Handles.inverseMatrix * (Transform.localToWorldMatrix * Center);
            set => Center = Transform.worldToLocalMatrix * (Handles.matrix * value);
        }

        public override Bounds HandleBounds
        {
            get => new Bounds(HandleCenter, HandleSize);
            set
            {
                HandleCenter = value.center;
                HandleSize = value.size;
            }
        }

        public override Vector3 WorldCenter
        {
            get => Transform.TransformPoint(CapsuleTarget.center);
            set => CapsuleTarget.center = Transform.InverseTransformPoint(value);
        }

        public override Bounds Bounds
        {
            get
            {
                return new Bounds(Center, Size);
            }
            set
            {
                Center = value.center;
                Size = value.size;
            }
        }

        public override Bounds WorldBounds => CapsuleTarget.bounds;

        private CapsuleCollider CapsuleTarget => (CapsuleCollider)m_Target;


        public CapsuleColliderProxy(CapsuleCollider capsuleCollider) : base(capsuleCollider) { }

        public override void Rotate(Vector3 axis)
        {
            int dir = CapsuleTarget.direction;
            var x = (int)Mathf.Abs(axis[s_RightDirId[dir]]);
            var y = (int)Mathf.Abs(axis[dir]);
            var z = (int)Mathf.Abs(axis[s_ForwardDirId[dir]]);
            var id = (x != 0) ? 0 : (y != 0) ? 1 : (z != 0) ? 2 : -1;
            if (id == -1)
                return;

            CapsuleTarget.direction = s_DirectionArray[dir, id];
        }

        public override void SetHandleMinMax(Vector3 min, Vector3 max)
        {
            Bounds prevBounds = HandleBounds;
            var nmin = Vector3.Min(min, prevBounds.max);
            var nmax = Vector3.Max(prevBounds.min, max);
            int dir = CapsuleTarget.direction;
            float minaxis = nmin[dir];
            float maxaxis = nmax[dir];
            float pmaxaxis = prevBounds.max[dir];
            float radius = prevBounds.size[s_RightDirId[dir]];
            nmin[dir] = Mathf.Min(minaxis, pmaxaxis - radius);
            nmax[dir] = Mathf.Max(nmin[dir] + radius, maxaxis);
            HandleCenter = (nmax + nmin) * 0.5f;
            HandleSize = (nmax - nmin);
        }

        internal float GetHandleRadiusAndHeight(out float handleHeight)
        {
            var handleSize = HandleSize;
            var dir = CapsuleTarget.direction;
            handleHeight = handleSize[dir];
            return handleSize[s_RightDirId[dir]] * 0.5f;
        }
    }

}
