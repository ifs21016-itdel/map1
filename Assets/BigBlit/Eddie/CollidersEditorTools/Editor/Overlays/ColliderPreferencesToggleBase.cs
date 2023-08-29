#if UNITY_2021_2_OR_NEWER

using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBlit.Eddie.CollidersEditorTools
{
    
    internal abstract class ColliderPreferencesToggleBase : EditorToolbarToggle
    {
        protected abstract string Name { get; }
        protected abstract string Tooltip { get; }
        protected abstract Texture2D Icon { get; }
        protected abstract PrefBool PrefValue { get; }

        public ColliderPreferencesToggleBase()
        {
            name = Name;
            tooltip = Tooltip;
            icon = Icon;

            this.RegisterValueChangedCallback((evt) =>
            {
                PrefValue.Value = evt.newValue;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            });

            RegisterCallback<AttachToPanelEvent>(new EventCallback<AttachToPanelEvent>(this.OnAttachedToPanel));
            RegisterCallback<DetachFromPanelEvent>(new EventCallback<DetachFromPanelEvent>(this.OnDetachFromPanel));
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            SetValueWithoutNotify(PrefValue.Value);
            PrefValue.OnValueChanged += setValue;
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) => PrefValue.OnValueChanged -= setValue;

        private void setValue(bool value) => SetValueWithoutNotify(value);
    }
}

#endif