using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace BigBlit.Eddie.CollidersEditorTools
{
    public static class ColliderPreferencesDrawer
    {
        public static void onGUI()
        {
            var settings = ColliderPreferences.instance;
            var fields = typeof(ColliderPreferences).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            EditorGUILayout.BeginVertical();
            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;
            GUILayout.ExpandWidth(true);
            EditorGUIUtility.labelWidth = 180.0f;

            EditorGUI.BeginChangeCheck();
            foreach (var field in fields)
            {
                switch (field.FieldType.Name)
                {
                    case "PrefBool":
                        var prefBool = (PrefBool)field.GetValue(settings);
                        prefBool.Value = EditorGUILayout.Toggle(new GUIContent(ObjectNames.NicifyVariableName(field.Name), prefBool.Tooltip), prefBool.Value);
                        break;
                    case "PrefColor":
                        var prefColor = (PrefColor)field.GetValue(settings);
                        prefColor.Value = EditorGUILayout.ColorField(new GUIContent(ObjectNames.NicifyVariableName(field.Name), prefColor.Tooltip), prefColor.Value);
                        break;
                    case "PrefEnum":
                        var prefEnum = (PrefEnum)field.GetValue(settings);
                        prefEnum.Value = EditorGUILayout.EnumPopup(new GUIContent(ObjectNames.NicifyVariableName(field.Name), prefEnum.Tooltip), prefEnum.Value);
                        break;
                    case "PrefFloat":
                        var prefFloat = (PrefFloat)field.GetValue(settings);
                        prefFloat.Value = EditorGUILayout.FloatField(new GUIContent(ObjectNames.NicifyVariableName(field.Name), prefFloat.Tooltip), prefFloat.Value);
                        break;
                    case "PrefRangedFloat":
                        var prefRangedFloat = (PrefRangedFloat)field.GetValue(settings);
                        prefRangedFloat.Value = EditorGUILayout.Slider(new GUIContent(ObjectNames.NicifyVariableName(field.Name), prefRangedFloat.Tooltip),
                        prefRangedFloat.Value, prefRangedFloat.Min, prefRangedFloat.Max);
                        break;
                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(settings);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel = prevIndent;
        }
    }
}
