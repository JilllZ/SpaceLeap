using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(DynamicCable))]
public class DynamicCableEditor : Editor {

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            DynamicCable dynamicCable = (DynamicCable)target;  

            dynamicCable.updateListSizes();

            for (int i = 0; i <= dynamicCable.wires; i++) {
                Vector3 currentPosition = dynamicCable.anchorPoints[i];
                Vector3 newPosition = Handles.PositionHandle(currentPosition + dynamicCable.transform.position, Quaternion.identity) - dynamicCable.transform.position;
                if (newPosition != currentPosition) {
                    dynamicCable.anchorPoints[i] = newPosition;
                    EditorUtility.SetDirty(dynamicCable);
                }
            }

            dynamicCable.updateMesh();

            Undo.RecordObject(dynamicCable, "Adjusted dynamic cable");
        }
    }
}