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

    public override string getVariant(int variantIndex) {
        return _instructionBase.Replace(REPLACEMENT_CODE, _variantReplacements[variantIndex]);
    }

    public override int getVariantCount() {
        return _variantReplacements.Length;
    }
}
