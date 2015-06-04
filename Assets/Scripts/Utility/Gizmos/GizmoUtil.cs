using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class GizmoUtil {
    private static Stack<Matrix4x4> _matrixStack = new Stack<Matrix4x4>();
    public static bool ingameEnabled = false;

    public static void pushMatrix() {
        _matrixStack.Push(Gizmos.matrix);
    }

    public static void popMatrix() {
        Gizmos.matrix = _matrixStack.Pop();
    }

    public static void relativeTo(GameObject obj) {
        Gizmos.matrix = obj.transform.localToWorldMatrix;
    }

    public static void DrawCube(Vector3 center, Vector3 size) {
#if UNITY_EDITOR
        if (!ingameEnabled) {
            Gizmos.DrawCube(center, size);
        }
        if (Application.isPlaying && ingameEnabled) {
#endif
            GL.Begin(GL.QUADS);
            GL.Color(Gizmos.color);

            Vector3 radius = size / 2.0f;

            GL.Vertex(center + new Vector3(radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, radius.z));
            GL.Vertex(center + new Vector3(radius.x, -radius.y, radius.z));

            GL.Vertex(center + new Vector3(radius.x, radius.y, -radius.z));
            GL.Vertex(center + new Vector3(-radius.x, radius.y, -radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, -radius.z));
            GL.Vertex(center + new Vector3(radius.x, -radius.y, -radius.z));

            GL.Vertex(center + new Vector3(radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, radius.y, -radius.z));
            GL.Vertex(center + new Vector3(radius.x, radius.y, -radius.z));

            GL.Vertex(center + new Vector3(radius.x, -radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, -radius.z));
            GL.Vertex(center + new Vector3(radius.x, -radius.y, -radius.z));

            GL.Vertex(center + new Vector3(radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(radius.x, -radius.y, radius.z));
            GL.Vertex(center + new Vector3(radius.x, -radius.y, -radius.z));
            GL.Vertex(center + new Vector3(radius.x, radius.y, -radius.z));

            GL.Vertex(center + new Vector3(-radius.x, radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, radius.z));
            GL.Vertex(center + new Vector3(-radius.x, -radius.y, -radius.z));
            GL.Vertex(center + new Vector3(-radius.x, radius.y, -radius.z));
            GL.End();
#if UNITY_EDITOR
        }
#endif
    }

    public static void DrawLine(Vector3 from, Vector3 to) {
#if UNITY_EDITOR
        if (!ingameEnabled) {
            Gizmos.DrawLine(from, to);
        }
        if (Application.isPlaying && ingameEnabled) {
#endif
            GL.Begin(GL.LINES);
            GL.Color(Gizmos.color);
            GL.Vertex(from);
            GL.Vertex(to);
            GL.End();
#if UNITY_EDITOR
        }
#endif
    }
}
