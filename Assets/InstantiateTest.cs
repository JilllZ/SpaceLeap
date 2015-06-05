using UnityEngine;
using System.Collections;

public class InstantiateTest : MonoBehaviour {
    public InteractionPanel prefab;

    void Start() {
        Instantiate(prefab);
    }
}
