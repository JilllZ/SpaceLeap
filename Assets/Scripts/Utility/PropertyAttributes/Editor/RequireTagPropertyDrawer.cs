using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(RequireTag))]
public class RequireTagPropertyDrawer : PropertyDrawer {
    private bool _hasSearched = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.ObjectReference) {
            Debug.LogWarning("Can only use RequireTag on a GameObject field!");
            return;
        }

        Object obj = property.objectReferenceValue;
        if (obj != null && obj.GetType() != typeof(GameObject)) {
            Debug.LogWarning("Can only use RequireTag on a GameObject field!");
            return;
        }

        RequireTag requireTag = attribute as RequireTag;
        GameObject selectedObj = obj as GameObject;

        if (selectedObj != null && selectedObj.tag != requireTag.tag) {
            selectedObj = null;
            Debug.LogWarning(selectedObj + " did not have the required tag [" + requireTag.tag + "] !");
        }

        if (selectedObj == null && !_hasSearched) {
            selectedObj = GameObject.FindGameObjectWithTag(requireTag.tag);
            _hasSearched = true;
        }

        property.objectReferenceValue = selectedObj;

        EditorGUI.PropertyField(position, property, label);
    }
}
