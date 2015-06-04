using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class DynamicCable : MonoBehaviour {
    [MinValue(1)]
    public int wires = 1;
    [MinValue(0)]
    public float sagAmount = 0.5f;
    [MinValue(0)]
    public float wireRadius = 0.1f;
    [MinValue(1)]
    public int segmentsPerWire = 8;
    [MinValue(3)]
    public int segmentResolution = 6;

    [HideInInspector]
    public List<Vector3> anchorPoints = new List<Vector3>();

    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private List<Vector3> _vertices = new List<Vector3>();
    private List<Vector2> _uvs = new List<Vector2>();
    private List<int> _triangles = new List<int>();

    // Use this for initialization
    void Awake() {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _mesh.name = "DynamicCableMesh";
        _mesh.hideFlags = HideFlags.HideAndDontSave;
        _meshFilter.sharedMesh = _mesh;

        updateMesh();

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null) {
            meshCollider.sharedMesh = _mesh;
        }
    }

    public void updateMesh() {
        updateListSizes();

        _mesh.Clear();
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();

        for (int i = 0; i < wires; i++) {
            for (int j = 0; j <= segmentsPerWire; j++) {
                int segmentIndex = i * segmentsPerWire + j;

                Vector3 position = getMeshRingCenter(segmentIndex);
                Vector3 direction;
                if (j == 0) {
                    direction = getMeshRingDirectionStart(segmentIndex);
                } else if (j == segmentsPerWire) {
                    direction = getMeshRingDirectionEnd(segmentIndex);
                } else {
                    direction = getMeshRingDirectionCenter(segmentIndex);
                }

                addVertexRing(position, direction, ((float)j) / segmentsPerWire, j != 0);
            }
        }

        Vector3[] vertices = _vertices.ToArray();
        Vector2[] uvs = _uvs.ToArray();
        int[] triangles = _triangles.ToArray();

        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private void addVertexRing(Vector3 center, Vector3 direction, float xuv, bool connectTris) {
        Vector3 up = new Vector3(0, 1, 0);
        Vector3 vertexOffsetStart = Vector3.Cross(direction, up).normalized * wireRadius;

        if (connectTris) {
            for (int vertex = 0; vertex < segmentResolution; vertex++) {
                int curVertex = vertex + _vertices.Count - (segmentResolution + 1);
                int nextVertex = curVertex + 1;

                _triangles.Add(curVertex);
                _triangles.Add(nextVertex + segmentResolution + 1);
                _triangles.Add(curVertex + segmentResolution + 1);

                _triangles.Add(curVertex);
                _triangles.Add(nextVertex);
                _triangles.Add(nextVertex + segmentResolution + 1);
            }
        }

        for (int vertex = 0; vertex <= segmentResolution; vertex++) {
            float angle = 360f * vertex / segmentResolution;
            Quaternion quat = Quaternion.AngleAxis(angle, direction);

            Vector3 vertexOffset = quat * vertexOffsetStart;
            Vector3 vertexPosition = center + vertexOffset;
            _vertices.Add(vertexPosition);

            Vector2 uv = new Vector2(xuv, ((float)vertex) / segmentResolution);
            _uvs.Add(uv);
        }
    }

    private Vector3 getAnchorPoint(int point) {
        return anchorPoints[point];
    }

    private Vector3 getMeshRingCenter(int index) {
        //return Vector3.zero;
        int startAnchor = index / segmentsPerWire;
        int endAnchor = startAnchor + 1;

        float percent = (index - startAnchor * segmentsPerWire) / ((float)segmentsPerWire);

        if (index == wires * segmentsPerWire) {
            return getAnchorPoint(startAnchor);
        } else {
            float sagPercent = 1f - 4f * (percent - 0.5f) * (percent - 0.5f);

            Vector3 position = Vector3.Lerp(getAnchorPoint(startAnchor), getAnchorPoint(endAnchor), percent);
            position = Vector3.Lerp(position, position - new Vector3(0, sagAmount, 0), sagPercent);

            return position;
        }
    }

    private Vector3 getMeshRingDirectionCenter(int index) {
        return Vector3.Lerp(getMeshRingDirectionStart(index), getMeshRingDirectionEnd(index), 0.5f).normalized;
    }

    private Vector3 getMeshRingDirectionStart(int index) {
        return (getMeshRingCenter(index + 1) - getMeshRingCenter(index)).normalized;
    }

    private Vector3 getMeshRingDirectionEnd(int index) {
        return (getMeshRingCenter(index) - getMeshRingCenter(index - 1)).normalized;
    }

    public void updateListSizes() {
        while (anchorPoints.Count <= wires) {
            if (anchorPoints.Count == 0) {
                anchorPoints.Add(new Vector3(0, 1, 0));
            } else if (anchorPoints.Count == 1) {
                anchorPoints.Add(anchorPoints[0] + new Vector3(1, 0, 0));
            } else {
                int index = anchorPoints.Count - 1;
                anchorPoints.Add(anchorPoints[index] + (anchorPoints[index] - anchorPoints[index - 1]));
            }
        }
    }
}