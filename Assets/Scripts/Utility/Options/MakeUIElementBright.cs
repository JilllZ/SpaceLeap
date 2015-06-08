using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class MakeUIElementBright : MonoBehaviour {
    public Color baseColor;
    public float multiplier = 1;

    void Update() {
        if (!Application.isPlaying) {
            GetComponent<Image>().color = baseColor * multiplier;
        }
    }
}
