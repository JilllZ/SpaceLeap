using UnityEngine;
using System.Collections;

public class MinValue : PropertyAttribute {
    public float minValue;

    public MinValue(float minValue) {
        this.minValue = minValue;
    }
}