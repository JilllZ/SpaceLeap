using UnityEngine;
using System.Collections;

public class MinMax : PropertyAttribute{
    public readonly float min, max;
    public readonly bool isInt;

    public MinMax(float min, float max) {
        this.min = min;
        this.max = max;
        isInt = false;
    }

    public MinMax(int min, int max) {
        this.min = min;
        this.max = max;
        isInt = true;
    }
}
