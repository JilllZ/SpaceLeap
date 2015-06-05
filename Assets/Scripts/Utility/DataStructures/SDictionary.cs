using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SDictionary<T,K> : Dictionary<T,K>, ISerializationCallbackReceiver {
    //They can be successfully serialized, but not edited in the inspector since you can only edit one at a time
    //All the intermediate serialization makes it super weird
    [SerializeField]
    [HideInInspector]
    private List<T> _serializedKeys = new List<T>();
    [SerializeField]
    [HideInInspector]
    private List<K> _serializedValues = new List<K>();

    public void OnBeforeSerialize() {
        _serializedKeys.Clear();
        _serializedValues.Clear();

        foreach (var pair in this) {
            _serializedKeys.Add(pair.Key);
            _serializedValues.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize() {
        this.Clear();
        for (int i = 0; i < _serializedKeys.Count; i++) {
            this[_serializedKeys[i]] = _serializedValues[i];
        }
    }
} 