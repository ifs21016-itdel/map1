using System;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal interface ICollider
    {
        Collider Target { get; }
        GameObject GameObject { get; }
        Transform Transform { get; }
        Type TargetType { get; }
        bool IsTargetValid { get; }

        Vector3 Center { get; set; }
        Vector3 Size { get; set; }
        Bounds Bounds {get; set;}
        Vector3 WorldCenter { get; set;}  
        Bounds WorldBounds { get; }
        Vector3 HandleCenter { get; set; }  // Properly rotated Local * lossyScale
        Vector3 HandleSize { get; set; }    // Local * LossyScale
        Bounds HandleBounds {get; set;}

        void Rotate(Vector3 angles);
        void SetHandleMinMax(Vector3 min, Vector3 max);

    }

}
