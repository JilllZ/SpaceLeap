using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(Incrementable))]
public class IncrementablePropertyDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position.width -= 40;
        EditorGUI.PropertyField(position, property, label, false);

        Rect minusButtonPos = position;
        minusButtonPos.x += position.width;
        minusButtonPos.width = 20;

        Rect plusButtonPos = position;
        plusButtonPos.x += position.width + 20;
        plusButtonPos.width = 20;

        if (GUI.Button(minusButtonPos, "-")) {
            property.intValue--;
        }
        if (GUI.Button(plusButtonPos, "+")) {
            property.intValue++;
        }
    }
}