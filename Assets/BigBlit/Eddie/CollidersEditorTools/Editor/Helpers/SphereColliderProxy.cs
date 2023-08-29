using UnityEngine;
using UnityEditor;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class SphereColliderProxy : ColliderProxy
    {
        public override Vector3 Center
        {
            get => SphereTarget.center;
            set => SphereTarget.center = value;
        }

        public override Vector3 Size
        {
            get => Vector3.one * Mathf.Max(Mathf.Abs(SphereTarget.radius) * 2.0f, Mathf.Epsilon);
            set
            {
                var nradius = ColliderHandleUtility.Vec3Abs(value) * 0.5f;
                var dradius = ColliderHandleUtility.Vec3Abs(nradius - Vector3.one * SphereTarget.radius);
                int ii = 0;
                for (int i = 0; i < 3; i++)
                {
                    if (dradius[i] > dradius[ii])
                        ii = i;
                }

                SphereTarget.radius = Mathf.Max(nradius[ii], Mathf.Epsilon);
            }
        }

        public override Bounds Bounds
        {
            get => new Bounds(SphereTarget.center, Size);
            set
            {
                Center = value.center;
                Size = value.size;
            }
        }

        public override Vector3 WorldCenter
        {
            get => Transform.TransformPoint(Center);
            set => Center = Transform.InverseTransformPoint(value);
        }

        public override Vector3 HandleCenter
        {
            get => Handles.inverseMatrix * (Transform.localToWorldMatrix * Center);
            set => Center = Transform.worldToLocalMatrix * (Handles.matrix * value);
        }

        public override Vector3 HandleSize
        {
            get => Size * ColliderHandleUtility.Vec3ToFloatMax(ColliderHandleUtility.GetLossyScaleAbs(m_Target));
            set => Size = ColliderHandleUtility.Vec3Abs(value) / ColliderHandleUtility.Vec3ToFloatMax(ColliderHandleUtility.Vec3Abs(Transform.lossyScale));
        }


        private SphereCollider SphereTarget => (SphereCollider)m_Target;


        public override Bounds WorldBounds => SphereTarget.bounds;

        public override Bounds HandleBounds
        {
            get => new Bounds(HandleCenter, HandleSize);
            set
            {
                HandleCenter = value.center;
                HandleSize = value.size;
            }
        }

        public float HandleRadius => HandleSize.x * 0.5f;

        public float Radius => SphereTarget.radius;


        public SphereColliderProxy(SphereCollider sphereCollider) : base(sphereCollider) { }

        public override void Rotate(Vector3 rotAngles)
        {

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
