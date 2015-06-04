using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer (typeof (MinMax))]
public class MinMaxPropertyDrawer : PropertyDrawer {
    public const float PERCENT_NUM = 0.2f;
    public const float SPACING = 3;


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        MinMax minMax = attribute as MinMax;

        if (property.propertyType != SerializedPropertyType.Vector2) {
            EditorGUI.PropertyField(position, property);
            Debug.LogWarning("The MinMax property can only be used on Vector2!");
            return;
        }

        Vector2 value = property.vector2Value;

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        float w = position.width * PERCENT_NUM;

        Rect leftNum = new Rect(position.x, position.y, w, position.height);
        Rect slider = new Rect(position.x + w + SPACING, position.y, position.width - 2 * w - SPACING * 2, position.height);
        Rect rightNum = new Rect(position.x + position.width - w, position.y, w, position.height);

        float newMin = EditorGUI.FloatField(leftNum, value.x);
        float newMax = EditorGUI.FloatField(rightNum, value.y);

        value.x = Mathf.Clamp(newMin, minMax.min, value.y);
        value.y = Mathf.Clamp(newMax, value.x, minMax.max);

        EditorGUI.MinMaxSlider(slider, ref value.x, ref value.y, minMax.min, minMax.max);

        property.vector2Value = value;

        EditorGUI.EndProperty();
    }
}