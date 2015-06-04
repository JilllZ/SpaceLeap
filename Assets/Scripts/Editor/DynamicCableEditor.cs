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
                dynamicCable.anchorPoints[i] = Handles.PositionHandle(dynamicCable.anchorPoints[i] + dynamicCable.transform.position, Quaternion.identity) - dynamicCable.transform.position;
            }

            dynamicCable.updateMesh();
        }
    }
}