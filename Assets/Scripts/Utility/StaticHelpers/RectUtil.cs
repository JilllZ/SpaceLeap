using UnityEngine;
using System.Collections;

public static class RectUtil {

    public static Vector2 random(this Rect rect) {
        Vector2 v = rect.size;
        v.Scale(new Vector2(Random.value - 0.5f, Random.value - 0.5f));
        return rect.center + v;
    }
}
