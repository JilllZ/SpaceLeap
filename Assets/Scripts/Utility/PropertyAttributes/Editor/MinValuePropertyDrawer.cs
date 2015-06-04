using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(MinValue))]
public class MinValuePropertyDrawer : PropertyDrawer {
    IncrementablePropertyDrawer intDrawer = new IncrementablePropertyDrawer();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        MinValue minVal = (MinValue)attribute;

        if (property.propertyType == SerializedPropertyType.Integer) {
            intDrawer.OnGUI(position, property, label);
            if (property.intValue < minVal.minValue) {
                property.intValue = (int)(minVal.minValue);
            }
        }
        if (property.propertyType == SerializedPropertyType.Float) {
            EditorGUI.PropertyField(position, property, label, false);
            if (property.floatValue < minVal.minValue) {
                property.floatValue = minVal.minValue;
            }
        }
    }
}