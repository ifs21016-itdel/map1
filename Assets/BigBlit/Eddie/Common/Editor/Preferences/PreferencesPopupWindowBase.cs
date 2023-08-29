using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BigBlit.Eddie
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public abstract class PreferencesPopupWindowBase : EditorWindow
    {
        protected virtual string PaneTitle => "PaneTitle";
        protected virtual Vector2 DefaultPosition => new Vector2(200, 200);
        protected virtual Vector2 DefaultSize => new Vector2(300, 500);

        private VisualTreeAsset UXML => _uxml ?? EddieUtility.LoadAssetAtPath<VisualTreeAsset>(k_UXMLPath);

        private const float k_BorderWidth = 1f;
        private const string k_UXMLPath = "Common/Editor/UXML/PreferencesPopupWindowBase.uxml";

        private bool m_isDown;
        private Vector2 m_prevPos;

        private VisualTreeAsset _uxml;


        public static T Show<T>() where T : PreferencesPopupWindowBase
        {
            T[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll<T>();
            T window = objectsOfTypeAll.Length != 0 ? (T)objectsOfTypeAll[0] : (T)null;
            if (window != null)
                window.Close();

            window = ScriptableObject.CreateInstance<T>();
            window.ShowPopup();

            return window;
        }


        protected virtual void AddOptionsMenuElements(GenericMenu menu) { }

        protected virtual void OnReset() { }

        protected virtual VisualElement CreateContent() => null;

        protected virtual void OnLoadPreferences() { }

        protected virtual void OnSavePreferences() { }

        protected virtual void OnEnable()
        {
            loadPreferences();
            setInlineStyles();
            prepareContainer();

            var content = CreateContent();
            if (content != null)
                rootVisualElement.Q(name: "Content").Add(content);
        }

        protected virtual void OnDisable()
        {
            rootVisualElement.Q<Button>(name: "PaneClose").clicked -= onClose;
            rootVisualElement.Q<Button>("PaneOptions").clicked -= showOptions;
            savePreferences();
        }

        private void onClose() => Close();

        private void OnEnableINTERNAL() => AssemblyReloadEvents.beforeAssemblyReload += new AssemblyReloadEvents.AssemblyReloadCallback(this.Close);

        private void OnDisableINTERNAL() => AssemblyReloadEvents.beforeAssemblyReload -= new AssemblyReloadEvents.AssemblyReloadCallback(this.Close);

        private void loadPreferences()
        {
            position = new Rect(EditorPrefs.GetFloat("EddiePreferencesPopupWindow" + "x", this.position.x),
                      EditorPrefs.GetFloat("EddiePreferencesPopupWindow" + "y", this.position.y),
                      EditorPrefs.GetFloat("EddiePreferencesPopupWindow" + "w", this.position.width),
                      EditorPrefs.GetFloat("EddiePreferencesPopupWindow" + "h", this.position.height));

            OnLoadPreferences();

        }

        private void savePreferences()
        {

            EditorPrefs.SetFloat("EddiePreferencesPopupWindow" + "x", this.position.x);
            EditorPrefs.SetFloat("EddiePreferencesPopupWindow" + "y", this.position.y);
            EditorPrefs.SetFloat("EddiePreferencesPopupWindow" + "w", this.position.width);
            EditorPrefs.SetFloat("EddiePreferencesPopupWindow" + "h", this.position.height);

            OnSavePreferences();
        }

        private void showOptions()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(EditorGUIUtility.TrTextContent("Reset"), false, () =>
            {
                position = new Rect(position.min, DefaultSize);
                OnReset();
                savePreferences();
            });

            genericMenu.AddSeparator("");
            AddOptionsMenuElements(genericMenu);
            genericMenu.ShowAsContext();
        }

        private void setInlineStyles()
        {
            Color color = EditorGUIUtility.isProSkin ? new Color(0.44f, 0.44f, 0.44f, 1f) : new Color(0.51f, 0.51f, 0.51f);
            rootVisualElement.style.borderLeftWidth = (StyleFloat)1f;
            rootVisualElement.style.borderTopWidth = (StyleFloat)1f;
            rootVisualElement.style.borderRightWidth = (StyleFloat)1f;
            rootVisualElement.style.borderBottomWidth = (StyleFloat)1f;
            rootVisualElement.style.borderLeftColor = (StyleColor)color;
            rootVisualElement.style.borderTopColor = (StyleColor)color;
            rootVisualElement.style.borderRightColor = (StyleColor)color;
            rootVisualElement.style.borderBottomColor = (StyleColor)color;
        }

        private void prepareContainer()
        {
            UXML.CloneTree(rootVisualElement);

            var paneTitle = rootVisualElement.Q<TextElement>("PaneTitle");
            if (paneTitle != null)
            {
                paneTitle.text = PaneTitle;
                paneTitle.RegisterCallback<MouseDownEvent>((evt) =>
                {
                    m_prevPos = GUIUtility.GUIToScreenPoint(evt.mousePosition);
                    evt.target.CaptureMouse();
                    m_isDown = true;
                });
                paneTitle.RegisterCallback<MouseUpEvent>((evt) =>
              {
                  evt.target.ReleaseMouse();

                  m_isDown = false;
              });

                paneTitle.RegisterCallback<MouseMoveEvent>((evt) =>
             {
                 if (!m_isDown)
                     return;
                 Vector2 pos = GUIUtility.GUIToScreenPoint(evt.mousePosition);
                 Vector2 delta = pos - m_prevPos;
                 m_prevPos = pos;
                 position = new Rect(position.min + delta, position.size);
             });

            }

             rootVisualElement.Q<Button>(name: "PaneClose").style.backgroundImage = EditorGUIUtility.isProSkin 
             ? EddieUtility.LoadAssetAtPath<Texture2D>("Common/Editor/Resources/d_CloseIcon.png")
             : EddieUtility.LoadAssetAtPath<Texture2D>("Common/Editor/Resources/l_CloseIcon.png");

            rootVisualElement.Q<Button>(name: "PaneClose").clicked += onClose;
            rootVisualElement.Q<Button>("PaneOptions").clicked += showOptions;
        }
    }
}
