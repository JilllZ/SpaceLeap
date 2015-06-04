using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class RenderGizmosInGame : MonoBehaviour {
    private Dictionary<Component, MethodInfo> _gizmoMethods = new Dictionary<Component, MethodInfo>();

    private Material _mat;

    void OnEnable() {
        Component[] cs = GetComponents<Component>();
        foreach (Component c in cs) {
            MethodInfo[] methods = c.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo m in methods) {
                if (m.Name == "OnDrawGizmos") {
                    _gizmoMethods[c] = m;
                    break;
                }
            }
        }

        _mat = new Material(Shader.Find("Hidden/GizmoOverlay"));
    }

    void OnRenderObject() {
        GizmoUtil.ingameEnabled = true;
        GL.LoadProjectionMatrix(Camera.current.projectionMatrix);
        _mat.SetPass(0);

        object[] emptyArgs = new object[0];
        foreach (var pair in _gizmoMethods) {
            pair.Value.Invoke(pair.Key, emptyArgs);
        }

        GizmoUtil.ingameEnabled = false;
    }
}
