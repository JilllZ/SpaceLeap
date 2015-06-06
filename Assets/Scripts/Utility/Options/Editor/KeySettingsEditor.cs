using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(KeySettings))]
public class KeySettingsEditor : Editor {
    private static string[] keyCodeStrings = null;
    private static GUIStyle _disabledStyle;

    void OnEnable() {
        if (keyCodeStrings == null) {
            keyCodeStrings = System.Enum.GetNames(typeof(KeyCode));
        }
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

        SerializedProperty dataDictionary = serializedObject.FindProperty("allSettingData");
        SerializedProperty dataValueList = dataDictionary.FindPropertyRelative("_serializedValues");

        if (dataValueList.arraySize != 0) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable All Classes")) {
                ((KeySettings)target).enableAll();
            }
            if (GUILayout.Button("Disable All Classes")) {
                ((KeySettings)target).disableAll();
            }
            if (GUILayout.Button("Reset Classes")) {
                ((KeySettings)target).resetEnabled();
            }
            GUILayout.EndHorizontal();
        }

        HashSet<int> existing = new HashSet<int>();
        HashSet<int> duplicate = new HashSet<int>();

        for (int i = 0; i < dataValueList.arraySize; i++) {
            SerializedProperty group = dataValueList.GetArrayElementAtIndex(i);
            SerializedProperty enabledProperty = group.FindPropertyRelative("enabled");
            if (!enabledProperty.boolValue) {
                continue;
            }

            SerializedProperty keyCodeDictionary = group.FindPropertyRelative("fields");
            SerializedProperty keyCodeValueList = keyCodeDictionary.FindPropertyRelative("_serializedValues");

            for (int j = 0; j < keyCodeValueList.arraySize; j++) {
                SerializedProperty setting = keyCodeValueList.GetArrayElementAtIndex(j);
                SerializedProperty keyCode = setting.FindPropertyRelative("keyCode");

                if (keyCode.enumValueIndex == 0) {
                    continue;
                }

                if (existing.Contains(keyCode.enumValueIndex)) {
                    duplicate.Add(keyCode.enumValueIndex);
                } else {
                    existing.Add(keyCode.enumValueIndex);
                }
            }
        }

        for (int i = 0; i < dataValueList.arraySize; i++) {
            SerializedProperty group = dataValueList.GetArrayElementAtIndex(i);
            SerializedProperty name = group.FindPropertyRelative("friendlyName");
            SerializedProperty isEnabled = group.FindPropertyRelative("enabled");

            if (_disabledStyle == null) {
                _disabledStyle = new GUIStyle(EditorStyles.foldout);
                _disabledStyle.normal.textColor = Color.gray;
            }
            isEnabled.boolValue = EditorGUILayout.Foldout(isEnabled.boolValue, name.stringValue + (isEnabled.boolValue ? "" : " (disabled)"), _disabledStyle);

            if (isEnabled.boolValue) {
                SerializedProperty keyCodeDictionary = group.FindPropertyRelative("fields");
                SerializedProperty keyCodeValueList = keyCodeDictionary.FindPropertyRelative("_serializedValues");

                if (Event.current.type == EventType.ContextClick &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) {
                    GenericMenu contextMenu = new GenericMenu();
                    contextMenu.AddItem(new GUIContent("Reset All To Default"), false, ((KeySettings)target).resetClass, group.FindPropertyRelative("fullTypeName").stringValue);
                    contextMenu.ShowAsContext();
                }

                for (int j = 0; j < keyCodeValueList.arraySize; j++) {
                    SerializedProperty setting = keyCodeValueList.GetArrayElementAtIndex(j);
                    SerializedProperty varName = setting.FindPropertyRelative("fieldName");
                    SerializedProperty keyCode = setting.FindPropertyRelative("keyCode");

                    if (duplicate.Contains(keyCode.enumValueIndex)) {
                        GUI.color = Color.yellow;
                    }

                    keyCode.enumValueIndex = EditorGUILayout.Popup(varName.stringValue, keyCode.enumValueIndex, keyCodeStrings);

                    GUI.color = Color.white;
                }
            }

            
        }

        serializedObject.ApplyModifiedProperties();
    }
}