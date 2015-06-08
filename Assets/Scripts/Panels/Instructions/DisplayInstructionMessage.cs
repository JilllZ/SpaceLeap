using UnityEngine;
using System.Collections;

public class DisplayInstructionMessage : CustomMessage {
    public string instruction;
    public bool startTimer;

    public DisplayInstructionMessage() { }

    public DisplayInstructionMessage(string instruction, bool startTimer) {
        this.instruction = instruction;
        this.startTimer = startTimer;
    }
}
