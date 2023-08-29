using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class BoxColliderProxy : ColliderProxy
    {
        public override Vector3 Center
        {
            get => BoxTarget.center;
            set => BoxTarget.center = value;
        }

        public override Vector3 Size
        {
            get => ColliderHandleUtility.Vec3Abs(BoxTarget.size);
            set => BoxTarget.size = ColliderHandleUtility.Vec3Abs(value);
        }

        public override Bounds Bounds
        {
            get => new Bounds(BoxTarget.center, Size);
            set
            {
                BoxTarget.center = value.center;
                BoxTarget.size = ColliderHandleUtility.Vec3Abs(value.size);
            }
        }

        public override Vector3 WorldCenter
        {
            get => Transform.TransformPoint(BoxTarget.center);
            set => BoxTarget.center = Transform.InverseTransformPoint(value);
        }

        public override Vector3 HandleCenter
        {
            get => Handles.inverseMatrix * (Transform.localToWorldMatrix * BoxTarget.center);
            set => BoxTarget.center = Transform.worldToLocalMatrix * (Handles.matrix * value);
        }


        public override Vector3 HandleSize
        {
            get => ColliderHandleUtility.Vec3Abs(Vector3.Scale(BoxTarget.size, Transform.lossyScale));
            set => BoxTarget.size = ColliderHandleUtility.Vec3Abs(Vector3.Scale(value, ColliderHandleUtility.GetLossyScaleInverted(m_Target)));
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

        public override Bounds WorldBounds => BoxTarget.bounds;

        private BoxCollider BoxTarget => (BoxCollider)m_Target;

        public BoxColliderProxy(BoxCollider boxCollider) : base(boxCollider) {}

        public override void Rotate(Vector3 axis)
        {
            var quat = Quaternion.Euler(axis * 90.0f);
            HandleSize = quat * HandleSize;
        }

        public override void SetHandleMinMax(Vector3 min, Vector3 max)
        {
            Bounds prevBounds = HandleBounds;
            var nmin = Vector3.Min(min, prevBounds.max);
            var nmax = Vector3.Max(prevBounds.min, max);
            HandleCenter = (nmax + nmin) * 0.5f;
            HandleSize = (nmax - nmin);
        }
    }

}
