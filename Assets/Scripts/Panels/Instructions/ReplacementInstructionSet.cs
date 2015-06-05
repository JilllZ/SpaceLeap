using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ReplacementInstructionSet : MessageBase, IInstructionSet {
    public const string REPLACEMENT_CODE = "#";

    private string _instructionBase;
    private string[] _variantReplacements;

    public string getVariant(int variantIndex) {
        return _instructionBase.Replace(REPLACEMENT_CODE, _variantReplacements[variantIndex]);
    }

    public int getVariantCount() {
        return _variantReplacements.Length;
    }
}
