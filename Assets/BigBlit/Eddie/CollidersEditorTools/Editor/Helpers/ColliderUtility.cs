using UnityEngine;
using System;

namespace BigBlit.Eddie.CollidersEditorTools
{

    internal static class ColliderUtility
    {
        static string s_RootFolderName = "CollidersEditorTools";

        internal static ICollider CreateProxy(UnityEngine.Object target)
        {
            ICollider collider = null;
            Type type = target.GetType();
            if (type == typeof(BoxCollider))
                collider = new BoxColliderProxy((BoxCollider)target);
            else if (type == typeof(SphereCollider))
                collider = new SphereColliderProxy((SphereCollider)target);
            else if (type == typeof(CapsuleCollider))
                collider = new CapsuleColliderProxy((CapsuleCollider)target);

            return collider;
        }

        internal static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => EddieUtility.LoadAssetAtPath<T>(s_RootFolderName + "/" + path);

    }

}