using UnityEngine;
using System.Collections;

public abstract class PanelActionSetBase : CustomMessage {
    private int _setId;
    private int _currentVariantIndex;

    public int setId {
        get {
            return _setId;
        }
    }

    public int currentVariantIndex {
        get {
            return _currentVariantIndex;
        }
        set {
            _currentVariantIndex = value;
        }
    }

    public PanelActionSetBase() {
        _setId = Random.Range(int.MinValue, int.MaxValue);
    }

    public abstract string getVariant(int variantIndex);
    public abstract int getVariantCount();
}
