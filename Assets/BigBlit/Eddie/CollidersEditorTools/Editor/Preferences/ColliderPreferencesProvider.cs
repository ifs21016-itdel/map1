using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace BigBlit.Eddie.CollidersEditorTools
{
    public class ColliderPreferencesProvider : SettingsProvider
    {
        private SerializedObject m_serializedObject;


        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new ColliderPreferencesProvider();
            provider.keywords = GetSearchKeywordsFromSerializedObject(ColliderPreferences.GetSerialized());
            return provider;
        }


        public ColliderPreferencesProvider() : base("Preferences/Colliders Editor Tools", SettingsScope.User) { }

        public override void OnActivate(string searchContext, VisualElement rootElement) => m_serializedObject = ColliderPreferences.GetSerialized();

        public override void OnGUI(string searchContext)
        {
            ColliderPreferencesDrawer.onGUI();
            GUILayout.Space(8.0f);
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(128.0f)))
            {
                ColliderPreferences.instance.Reset();
                 UnityEditorInternal.InternalEditorUtility.RepaintAllViews();   
            }

        }
    }
}