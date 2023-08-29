#if UNITY_2021_2_OR_NEWER

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBlit.Eddie.CollidersEditorTools
{

    internal abstract class ColliderTransformToggleBase : EditorToolbarToggle
    {
        protected abstract string Name { get; }
        protected abstract string Tooltip { get; }
        protected abstract Texture2D Icon { get; }
        protected abstract Type ToolType { get; }


        public ColliderTransformToggleBase()
        {
            name = Name;
            tooltip = Tooltip;
            icon = Icon;

            this.RegisterValueChangedCallback((evt) =>
            {
                ColliderTransformToolManager.instance.SetActiveTool(evt.newValue == true ? ToolType : null);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });

            RegisterCallback<AttachToPanelEvent>(new EventCallback<AttachToPanelEvent>(this.onAttachedToPanel));
            RegisterCallback<DetachFromPanelEvent>(new EventCallback<DetachFromPanelEvent>(this.onDetachFromPanel));
        }

        private void onAttachedToPanel(AttachToPanelEvent evt)
        {
            SetEnabled(ColliderTransformToolManager.instance.IsActive);
            ColliderTransformToolManager.instance.stateChanged += colliderTransformToolManagerStateChanged;
            SetValueWithoutNotify(ColliderTransformToolManager.instance.ActiveToolType == ToolType);
            ColliderTransformToolManager.instance.transformToolChanged += setValue;
        }

        private void onDetachFromPanel(DetachFromPanelEvent evt)
        {
            ColliderTransformToolManager.instance.stateChanged -= colliderTransformToolManagerStateChanged;
            ColliderTransformToolManager.instance.transformToolChanged -= setValue;
        }

        private void colliderTransformToolManagerStateChanged() => SetEnabled(ColliderTransformToolManager.instance.IsActive);


        private void setValue() => SetValueWithoutNotify(ColliderTransformToolManager.instance.ActiveToolType == ToolType);

    }

}

#endif
