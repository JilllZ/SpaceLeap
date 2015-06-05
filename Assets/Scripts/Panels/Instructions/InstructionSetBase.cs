using UnityEngine;
using System.Collections;

public abstract class InstructionSetBase : CustomMessage {
    private int _setId;

    public int setId {
        get {
            return _setId;
        }
    }

    public InstructionSetBase() {
        _setId = Random.Range(int.MinValue, int.MaxValue);
    }

    public abstract string getVariant(int variantIndex);
    public abstract int getVariantCount();
}
