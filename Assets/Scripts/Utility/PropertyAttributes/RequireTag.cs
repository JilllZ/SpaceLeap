using UnityEngine;
using System.Collections;

public class RequireTag : PropertyAttribute {
    public readonly string tag;

    public RequireTag(string tag) {
        this.tag = tag;
    }
}
