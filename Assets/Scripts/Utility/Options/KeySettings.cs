using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class KeySettings : MonoBehaviour {
    public ClassSettingsDictionary allSettingData = new ClassSettingsDictionary();

    [System.Serializable]
    public class ClassSettingsDictionary : SDictionary<string, ClassSettingData> { }

    [System.Serializable]
    public class ClassSettingData {
        public string friendlyName;
        public string fullTypeName;
        public bool enabled = true;
        public FieldSettingsDictionary fields = new FieldSettingsDictionary();
    }

    [System.Serializable]
    public class FieldSettingsDictionary : SDictionary<string, FieldSettingData> { }

    [System.Serializable]
    public class FieldSettingData {
        public string fieldName;
        public KeyCode keyCode;
    }

#if UNITY_EDITOR
    public void resetEnabled() {
        foreach (var s in allSettingData.Values.Where(s => s.enabled)) {
            s.fields.Clear();
        }
        findAll();
    }

    public void enableAll() {
        foreach (var pair in allSettingData) {
            pair.Value.enabled = true;
        }
    }

    public void disableAll() {
        foreach (var pair in allSettingData) {
            pair.Value.enabled = false;
        }
    }

    public void resetClass(object fullClassName) {
        allSettingData.Remove((string)fullClassName);
        findAll();
    }

    private void findAll() {
        IEnumerable<MonoScript> assets = AssetDatabase.FindAssets("t:" + typeof(MonoScript).Name)
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)))
            .Select(obj => (MonoScript)obj);

        ClassSettingsDictionary previousSettings = allSettingData;
        allSettingData = new ClassSettingsDictionary();

        foreach (MonoScript script in assets) {
            System.Type type = script.GetClass();
            if (type == null) {
                continue;
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            ClassSettingData classSettingData;
            if (!previousSettings.TryGetValue(type.FullName, out classSettingData)) {
                classSettingData = new ClassSettingData();
                classSettingData.friendlyName = type.Name;
                classSettingData.fullTypeName = type.FullName;
                classSettingData.fields = new FieldSettingsDictionary();
            }

            foreach (FieldInfo field in fields) {
                if (field.FieldType == typeof(KeyCode)) {
                    FieldSettingData fieldSettingData;
                    if (!classSettingData.fields.TryGetValue(field.Name, out fieldSettingData)) {
                        fieldSettingData = new FieldSettingData();
                        fieldSettingData.fieldName = field.Name;
                        fieldSettingData.keyCode = (KeyCode)field.GetValue(null);
                    }

                    classSettingData.fields[field.Name] = fieldSettingData;
                }
            }

            if (classSettingData.fields.Count != 0) {
                allSettingData[type.FullName] = classSettingData;
            }
        }
    }
#endif

    void Awake() {
        if (Application.isPlaying) {
            Assembly assembly = Assembly.GetExecutingAssembly();

            foreach (ClassSettingData classSetting in allSettingData.Values) {
                System.Type type = assembly.GetType(classSetting.fullTypeName);
                if (type == null) continue;

                foreach (FieldSettingData fieldSetting in classSetting.fields.Values) {
                    FieldInfo field = type.GetField(fieldSetting.fieldName, BindingFlags.Static | BindingFlags.Public);
                    if (field == null) continue;

					if (classSetting.enabled) {
						field.SetValue(null, fieldSetting.keyCode);
					} else {
						field.SetValue(null, KeyCode.None);
					}
                }
            }
        } else {
#if UNITY_EDITOR
            KeySettings[] allKeySettings = FindObjectsOfType<KeySettings>();
            if (allKeySettings.Length != 1) {
                Debug.LogError("Cannot have more than one KeySettings component in the scene!");
                DestroyImmediate(this);
                return;
            }

            findAll();
#endif
        }
    }
}
 