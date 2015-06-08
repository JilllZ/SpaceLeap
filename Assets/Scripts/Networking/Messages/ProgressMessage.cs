using UnityEngine;
using System.Collections;

public class ProgressMessage : CustomMessage {
    public int progress;

    public ProgressMessage() { }

    public ProgressMessage(int progress) {
        this.progress = progress;
    }
}
