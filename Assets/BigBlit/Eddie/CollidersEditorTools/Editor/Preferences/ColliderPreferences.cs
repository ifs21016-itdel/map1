using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace BigBlit.Eddie.CollidersEditorTools
{
    [Serializable]
    public class PrefValue<T>
    {
        public event Action<T> OnValueChanged;

        public string Tooltip => m_Tooltip;

        public T Default => m_Default;

        public T Value
        {
            get => m_Value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(m_Value, value))
                    return;
                m_Value = value;
                OnValueChanged?.Invoke(m_Value);
            }
        }

        protected T m_Default;
        protected string m_Tooltip;

        [SerializeField]
        protected T m_Value;


        public PrefValue(T defaultValue, string tooltip)
        {
            m_Default = defaultValue;
            m_Value = defaultValue;
            m_Tooltip = tooltip;
        }


        public void SetValueWithoutNotify(T value) => m_Value = value;

        public void Reset() => Value = Default;

        public static implicit operator T(PrefValue<T> prefValue) => prefValue.Value;
    }

    [Serializable]
    public class PrefColor : PrefValue<Color>
    {
        public PrefColor(Color defaultValue, string tooltip) : base(defaultValue, tooltip) { }
    }

    [Serializable]
    public class PrefBool : PrefValue<bool>
    {
        public PrefBool(bool defaultValue, string tooltip) : base(defaultValue, tooltip)
        {
        }

        public bool Toggle()
        {
            Value = !Value;
            return Value;
        }
    }

    [Serializable]
    public class PrefEnum : PrefValue<Enum>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string m_SerializedEnum = "0";

        public PrefEnum(Enum defaultValue, string tooltip) : base(defaultValue, tooltip) => updateSerializedEnum();

        public void OnAfterDeserialize()
        {
            try
            {
                m_Value = (System.Enum)Enum.Parse(m_Value.GetType(), m_SerializedEnum, true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnBeforeSerialize() => updateSerializedEnum();

        private void updateSerializedEnum()
        {
            try
            {
                m_SerializedEnum = Enum.Format(m_Value.GetType(), m_Value, "d");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    [Serializable]
    public class PrefFloat : PrefValue<float>
    {
        public PrefFloat(float defaultValue, string tooltip) : base(defaultValue, tooltip) { }
    }

    [Serializable]
    public class PrefRangedFloat : PrefValue<float>
    {
        private float m_Min;
        private float m_Max;

        public float Min => m_Min;
        public float Max => m_Max;

        public PrefRangedFloat(float min, float max, float defaultValue, string tooltip) : base(defaultValue, tooltip)
        {
            m_Min = min;
            m_Max = max;
        }
    }

    [FilePath("BigBlit/Eddie/CollidersEditorTools.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class ColliderPreferences : ScriptableSingleton<ColliderPreferences>
    {
        public enum HandleTypes { Cube, Dot, Circle, Rectangle };

        private static Color s_HoveredColliderColor = new Color(145f, 238f, 142f, 50f) / (float)byte.MaxValue;
        private static Color s_SelectedColliderColor = new Color(145f, 238f, 142f, 210f) / (float)byte.MaxValue;
        private static Color s_BoundsHandleColor = new Color(145f, 238f, 142f, 240f) / (float)byte.MaxValue;

        private static string s_SelectedColliderColorTooltip = "Selected collider boundaries color.\nSet alpha to zero to turn off showing boundaries around selected colliders.";
        private static string s_HoveredColliderColorTooltip = "Hovered collider (mouse over) boundaries color.\nSet alpha to zero to turn off showing boundaries around hovered collider.";
        private static string m_BoundsHandleColorTooltip = "Color of the \"Edit Bounds\" handles.";
        private static string m_BoundsHandleAxesColorsTooltip = "If selected the\"Edit Bounds\" handles colors will be axes color coded (x - red, y - green, z - blue)";
        private static string m_BoundsHandleTypeTooltip = "Type of  \"Edit Bounds\" handles.";
        private static string m_BoundsHandleSizeTooltip = "Size of \"Edit Bounds\" handles.";
        private static string m_LinesThicknessTooltip = "Thickness of lines(ex. selected collide boundaries).";
        private static string m_PreciseModeFactorTooltip = "Slowdown factor for Precide Mode (Shift key) that slows down sensitivity of handles movement..";
        private static string m_ShowCenterlinesTooltip = "Show the lines on x,y,z axis passing through the center of the collider.";
        private static string m_CenterlinesAxesColorsTooltip = "If selected center lines colors will be axes color coded (x - red, y - green, z - blue.)";
        private static string m_HookShortcutsTooltip = "When Collider Editor Tools are active, capture keystrokes normally used for switching Unity move, scale, rotate and rect tools.";

        [SerializeField]
        private PrefColor m_SelectedColliderColor = new PrefColor(s_SelectedColliderColor, s_SelectedColliderColorTooltip);
        public PrefColor SelectedColliderColor => m_SelectedColliderColor;

        [SerializeField]
        private PrefColor m_HoveredColliderColor = new PrefColor(s_HoveredColliderColor, s_HoveredColliderColorTooltip);
        public PrefColor HoveredColliderColor => m_HoveredColliderColor;

        [SerializeField]
        private PrefColor m_BoundsHandleColor = new PrefColor(s_BoundsHandleColor, m_BoundsHandleColorTooltip);
        public PrefColor BoundsHandleColor => m_BoundsHandleColor;

        [SerializeField]
        private PrefBool m_BoundsHandleAxesColors = new PrefBool(true, m_BoundsHandleAxesColorsTooltip);
        public PrefBool BoundsHandleAxesColors => m_BoundsHandleAxesColors;

        [SerializeField]
        private PrefEnum m_BoundsHandleType = new PrefEnum(HandleTypes.Cube, m_BoundsHandleTypeTooltip);
        public PrefEnum BoundsHandlesType => m_BoundsHandleType;

        [SerializeField]
        private PrefRangedFloat m_BoundsHandleSize = new PrefRangedFloat(0.30f, 1.5f, 0.75f, m_BoundsHandleSizeTooltip);
        public PrefRangedFloat BoundsHandleSize => m_BoundsHandleSize;

        [SerializeField]
        private PrefRangedFloat m_LinesThickness = new PrefRangedFloat(1.0f, 4.0f, 2.0f, m_LinesThicknessTooltip);
        public PrefRangedFloat LinesThickness => m_LinesThickness;

        [SerializeField]
        private PrefRangedFloat m_PreciseModeFactor = new PrefRangedFloat(0.01f, 0.5f, 0.25f, m_PreciseModeFactorTooltip);
        public PrefRangedFloat PrecideModeFactor => m_PreciseModeFactor;

        [SerializeField]
        private PrefBool m_ShowCenterlines = new PrefBool(true, m_ShowCenterlinesTooltip);
        public PrefBool ShowCenterlines => m_ShowCenterlines;

        [SerializeField]
        private PrefBool m_CenterlinesAxesColors = new PrefBool(true, m_CenterlinesAxesColorsTooltip);
        public PrefBool CenterLinesAxesColors => m_CenterlinesAxesColors;

        [SerializeField]
        private PrefBool m_HookShortcuts = new PrefBool(true, m_HookShortcutsTooltip);
        public PrefBool HookShortcuts => m_HookShortcuts;

        [SerializeField]
        private PrefEnum m_LegacyToolbar = new PrefEnum(LegacyPreferencesToolbar.ToolbarLocation.Off, "Legacy Toolbar location.");
        public PrefEnum LegacyToolbar => m_LegacyToolbar;
        public static SerializedObject GetSerialized() => new SerializedObject(ColliderPreferences.instance);


        public void Reset() => doReset();

        public void Save() => Save(true);


        private void OnEnable()
        {
            hideFlags &= ~HideFlags.NotEditable;
            hideFlags &= ~HideFlags.HideInInspector;
        }

        private void OnDisable() => Save();

        private void doReset()
        {
            m_SelectedColliderColor.Reset();
            m_HoveredColliderColor.Reset();
            m_BoundsHandleColor.Reset();
            m_BoundsHandleAxesColors.Reset();
            m_BoundsHandleType.Reset();
            m_ShowCenterlines.Reset();
            m_CenterlinesAxesColors.Reset();
            m_HookShortcuts.Reset();
            m_PreciseModeFactor.Reset();
            m_LinesThickness.Reset();
            m_BoundsHandleSize.Reset();
            m_LegacyToolbar.Reset();
        }


    }
}