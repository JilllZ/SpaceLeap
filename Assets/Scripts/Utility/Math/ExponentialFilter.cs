using UnityEngine;
using System.Collections;

public class ExponentialFilter {
    public readonly float _targetFramerate;

    private float _weightedValue;
    private float _weight = 0.0f;
    private bool _first = true;

    public ExponentialFilter(float targetFramerate = 60.0f) {
        _targetFramerate = targetFramerate;
        reset();
    }

    public float value {
        get {
            return _weightedValue / _weight;
        }
    }

    public void reset() {
        _first = true;
        _weight = 0.0f;
    }

    public void reset(float value, float weight = 1.0f) {
        _weightedValue = value;
        _weight = weight;
        _first = false;
    }

    private void updateInternal(float value, float smoothStrength, float weight) {
        weight *= (1.0f - smoothStrength);
        _weightedValue *= smoothStrength;
        _weightedValue += value * weight;
        _weight *= smoothStrength;
        _weight += weight;
    }

    public void update(float value, float smoothStrength, float deltaTime = 1.0f, float weight = 1.0f) {
        if (_first) {
            reset(value, weight);
        } else {
            float dtExponent = deltaTime * _targetFramerate;
            smoothStrength = Mathf.Pow(smoothStrength, dtExponent);
            updateInternal(value, smoothStrength, weight);
        }
    }
}
