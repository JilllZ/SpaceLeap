using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class TestMessage : CustomMessage {
    public string message;

    public TestMessage() { }

    public TestMessage(string message) {
        this.message = message;
    }
}
