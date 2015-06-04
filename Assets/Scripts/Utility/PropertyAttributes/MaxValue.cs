using UnityEngine;
using System.Collections;

public class MaxValue : PropertyAttribute {
    public float maxValue;

    public MaxValue(float maxValue) {
        this.maxValue = maxValue;
    }
}