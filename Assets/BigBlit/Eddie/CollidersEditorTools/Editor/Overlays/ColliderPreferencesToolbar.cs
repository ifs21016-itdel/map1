#if UNITY_2021_2_OR_NEWER

using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;


namespace BigBlit.Eddie.CollidersEditorTools
{

    [Overlay(typeof(SceneView), "BigBlit/Eddie/Colliders/Preferences", "Colliders Editor Tools", true)]
    public class ColliderPreferencesToolbar : ToolbarOverlay
    {
        private const string k_Id = "BigBlit/Eddie/Colliders/Preferences";

        public ColliderPreferencesToolbar()
        : base(ColliderPreferencesMoveElement.k_Id,
            ColliderPreferencesRotateElement.k_Id,
            ColliderPreferencesScaleElement.k_Id,
            ColliderPreferencesEditBoundsElement.k_Id)
        {

        }

        [SerializeField]
        private bool m_Displayed;

        private bool Displayed
        {
            get => EditorPrefs.GetBool("Eddie/Colliders/ToolsOverlayDisplayed", true);
            set => EditorPrefs.SetBool("Eddie/Colliders/ToolsOverlayDisplayed", value);
        }

        public override void OnCreated()
        {
            base.OnCreated();

            updateDisplayed();
            ToolManager.activeToolChanged += updateDisplayed;
            EditorApplication.delayCall += onDelayedCall;
            this.displayedChanged += onDisplayedChanged;
            m_Displayed = Displayed;
        }

        public override void OnWillBeDestroyed()
        {

            this.displayed = m_Displayed;
            this.displayedChanged -= onDisplayedChanged;
            ToolManager.activeToolChanged -= updateDisplayed;
        }

        private void onDisplayedChanged(bool newDisplayed)
        {
            if (m_internalChange)
                return;

            Displayed = m_Displayed = newDisplayed;
        }

        private void onDelayedCall()
        {
            var field = typeof(Overlay).GetProperty("canvas", BindingFlags.NonPublic | BindingFlags.Instance);
            var canvas = field.GetValue(this);
            VisualElement canvasRoot = (VisualElement)typeof(Overlay).Assembly.GetType("UnityEditor.Overlays.OverlayCanvas")
                .GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(canvas);
            var root = canvasRoot.Q(name: "Colliders Editor Tools");
            root.userData = this;
            var manip = new ContextualMenuManipulator(menuBuilder);
            root.AddManipulator(manip);
        }

        private void menuBuilder(ContextualMenuPopulateEvent evt)
        {
            if (evt.menu.MenuItems().Count == 0)
                return;

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Preferences", (x) =>
            {
                ColliderPreferencesPopupWindow.Show<ColliderPreferencesPopupWindow>();
            });
        }

        private bool m_internalChange;

        private void updateDisplayed()
        {
            if (!m_Displayed)
            {
                this.displayed = m_Displayed;
                return;
            }

            m_internalChange = true;
            this.displayed = ToolManager.activeToolType == typeof(ColliderEditorTransformTools);
            m_internalChange = false;
        }
    }
}

#endif

