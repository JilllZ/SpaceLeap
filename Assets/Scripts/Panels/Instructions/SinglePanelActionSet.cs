using UnityEngine;
using System.Collections;

public class SinglePanelActionSet : PanelActionSetBase {
    protected string _actionInstruction;

    public override int currentVariantIndex {
        get {
            return base.currentVariantIndex;
        }
        set {
            base.currentVariantIndex = -1;
        }
    }

    public SinglePanelActionSet() { }

    public SinglePanelActionSet(string panelLabel, string actionInstruction)
        : base(panelLabel) {
        _actionInstruction = actionInstruction;
    }

    public override string getVariant(int variantIndex) {
        return _actionInstruction;
    }

    public override int getVariantCount() {
        return 1;
    }

    public override void Serialize(UnityEngine.Networking.NetworkWriter writer) {
        base.Serialize(writer);
        writer.Write(_actionInstruction);
    }

    public override void Deserialize(UnityEngine.Networking.NetworkReader reader) {
        base.Deserialize(reader);
        _actionInstruction = reader.ReadString();
    }
}
