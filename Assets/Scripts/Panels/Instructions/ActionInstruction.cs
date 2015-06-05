using UnityEngine;
using System.Collections;

public class ActionInstruction : InstructionSetBase {
    private string _actionInstruction;

    public ActionInstruction(string actionInstruction) {
        _actionInstruction = actionInstruction;
    }

    public override string getVariant(int variantIndex) {
        return _actionInstruction;
    }

    public override int getVariantCount() {
        return 1;
    }
}
