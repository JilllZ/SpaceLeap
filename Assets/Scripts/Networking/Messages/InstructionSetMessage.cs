using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class InstructionSetMessage : MessageBase {
    public abstract IInstructionSet getInstructionSet();
}
