using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(MaxValue))]
public class MaxValuePropertyDrawer : PropertyDrawer {
    IncrementablePropertyDrawer intDrawer = new IncrementablePropertyDrawer();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        MaxValue maxVal = (MaxValue)attribute;

        if (property.propertyType == SerializedPropertyType.Integer) {
            intDrawer.OnGUI(position, property, label);
            if (property.intValue > maxVal.maxValue) {
                property.intValue = (int)(maxVal.maxValue);
            }
        }
        if (property.propertyType == SerializedPropertyType.Float) {
            EditorGUI.PropertyField(position, property, label, false);
            if (property.floatValue > maxVal.maxValue) {
                property.floatValue = maxVal.maxValue;
            }
        }
    }
}