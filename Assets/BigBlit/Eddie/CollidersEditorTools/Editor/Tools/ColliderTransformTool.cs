using UnityEditor;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{
    internal class ColliderTransformTool : ColliderTool
    {
        public bool IsTransforming => m_HotControlId != 0;
        public int HotControlId => m_HotControlId;


        private int m_HotControlId;


        public virtual void OnTransformGUI(SceneView sceneView, Vector3 handlePosition, Quaternion handleRotation) { }

        public override void OnDeactivated()
        {
            if (!IsTransforming)
                return;

            m_HotControlId = 0;
            OnTransformFinished();
        }
        

        internal virtual bool TransformStageCheck(EventType prevEventType, int prevHotControl)
        {
            var eventType = (Event.current.GetTypeForControl(m_HotControlId) == prevEventType) ? EventType.Ignore : prevEventType;

            if (eventType == EventType.MouseDown)
            {
                if (GUIUtility.hotControl == prevHotControl)
                    return false;

                m_HotControlId = GUIUtility.hotControl;
                OnTransformStarted();
                return true;
            }
            else if (eventType == EventType.MouseUp || GUIUtility.hotControl != m_HotControlId)
            {
                m_HotControlId = 0;
                OnTransformFinished();
                return true;
            }

            return false;

        }


        protected virtual void OnTransformStarted() { }

        protected virtual void OnTransformFinished() { }

    }
}
