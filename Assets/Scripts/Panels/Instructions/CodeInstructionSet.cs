using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CodeInstructionSet : MessageBase, IInstructionSet {
    public const string REPLACEMENT_STRING = "#";
    public const short MESSAGE_ID = MsgType.Highest + 10;

    private string _instructionBase;
    private short _minNumber;
    private short _maxNumber;
    private byte _minDigits;

    public string getVariant(int variantIndex) {
        int number = _minNumber + variantIndex;
        string numberString = number.ToString().PadLeft(_minDigits, '0');
        return _instructionBase.Replace(REPLACEMENT_STRING, numberString);
    }

    public int getVariantCount() {
        return _maxNumber - _minNumber + 1;
    }
}
