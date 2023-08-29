using UnityEngine;
using UnityEngine.UIElements;

namespace BigBlit.Eddie.CollidersEditorTools
{
    public class ColliderPreferencesPopupWindow : PreferencesPopupWindowBase
    {
        protected override VisualElement CreateContent() => new IMGUIContainer(onGUI);

        protected override Vector2 DefaultSize => new Vector2(400, 500);

        protected override string PaneTitle => "Colliders Editor Tools - Preferences";

        protected override void OnReset()
        {
            ColliderPreferences.instance.Reset();

        }
        

        private void onGUI() 
        {
            ColliderPreferencesDrawer.onGUI();
        }

        protected override void OnSavePreferences()
        {
            ColliderPreferences.instance.Save();
        }
    }
}
