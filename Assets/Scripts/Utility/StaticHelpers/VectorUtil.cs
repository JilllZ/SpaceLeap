using UnityEngine;
using System.Collections;

public static class VectorUtil {

    public static Vector2 xy(this Vector3 v) {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 xz(this Vector3 v){
        return new Vector2(v.x, v.z);
    }

    public static Vector2 yz(this Vector3 v) {
        return new Vector2(v.y, v.z);
    }

    public static Vector3 randomWithinBounds(this Vector3 v) {
        v.Scale(new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f));
        return v;
    }

    public static Vector3 abs(this Vector3 v) {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector2 abs(this Vector2 v) {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    public static float maxc(this Vector3 v) {
        return Mathf.Max(v.x, v.y, v.z);
    }

    public static float maxc(this Vector2 v) {
        return Mathf.Max(v.x, v.y);
    }
}
