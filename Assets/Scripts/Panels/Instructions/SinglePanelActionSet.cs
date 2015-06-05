using UnityEngine;
using System.Collections;

public class SinglePanelActionSet : PanelActionSetBase {
    private string _actionInstruction;

    public SinglePanelActionSet() { }

    public SinglePanelActionSet(string actionInstruction) {
        _actionInstruction = actionInstruction;
    }

    public override string getVariant(int variantIndex) {
        return _actionInstruction;
    }

    public override int getVariantCount() {
        return 1;
    }
}
