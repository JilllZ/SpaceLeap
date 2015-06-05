using UnityEngine;
using System.Collections;

public class ActionInstruction : IInstructionSet {
    private string _actionInstruction;

    public ActionInstruction(string actionInstruction) {
        _actionInstruction = actionInstruction;
    }

    public string getVariant(int variantIndex) {
        return _actionInstruction;
    }

    public int getVariantCount() {
        return 1;
    }
}
