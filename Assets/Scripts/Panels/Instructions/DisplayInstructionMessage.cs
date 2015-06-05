using UnityEngine;
using System.Collections;

public class DisplayInstructionMessage : CustomMessage {
    public string instruction;

    public DisplayInstructionMessage() { }

    public DisplayInstructionMessage(string instruction) {
        this.instruction = instruction;
    }
}
