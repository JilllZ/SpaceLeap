using UnityEngine;
using System.Collections;

public static class BoundsUtil {

    public static Vector3 random(this Bounds bounds) {
        Vector3 v = bounds.size;
        v.Scale(new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f));
        return bounds.center + v;
    }
}
