using UnityEngine;
using System.Collections;

public abstract class PanelActionSetBase : CustomMessage {
    private int _setId;

    public int setId {
        get {
            return _setId;
        }
    }

    public PanelActionSetBase() {
        _setId = Random.Range(int.MinValue, int.MaxValue);
    }

    public abstract string getVariant(int variantIndex);
    public abstract int getVariantCount();
}
