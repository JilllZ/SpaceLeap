using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ReplacementPanelActionSet : PanelActionSetBase {
    public const string REPLACEMENT_CODE = "#";

    private string _instructionBase;
    private string[] _variantReplacements;

    public ReplacementPanelActionSet() { }

    public ReplacementPanelActionSet(string panelLabel, string instructionBase, params string[] variantReplacements)
        : base(panelLabel) {
        _instructionBase = instructionBase;
        _variantReplacements = variantReplacements;
    }

    public string getReplacement(int variantIndex) {
        return _variantReplacements[variantIndex];
    }

    public override string getVariant(int variantIndex) {
        return _instructionBase.Replace(REPLACEMENT_CODE, _variantReplacements[variantIndex]);
    }

    public override int getVariantCount() {
        return _variantReplacements.Length;
    }

    public override void Serialize(NetworkWriter writer) {
        base.Serialize(writer);
        writer.Write(_instructionBase);
        writer.Write(_variantReplacements.Length);
        foreach (string replacement in _variantReplacements) {
            writer.Write(replacement);
        }
    }

    public override void Deserialize(NetworkReader reader) {
        base.Deserialize(reader);
        _instructionBase = reader.ReadString();
        _variantReplacements = new string[reader.ReadInt32()];
        for (int i = 0; i < _variantReplacements.Length; i++) {
            _variantReplacements[i] = reader.ReadString();
        }
    }
}
