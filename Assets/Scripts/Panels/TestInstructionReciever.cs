using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class TestInstructionReciever : MonoBehaviour {

    void Awake() {
        CustomMessage.registerClientHandler<DisplayInstructionMessage>(instructionHandler);
    }

    private void instructionHandler(NetworkMessage message) {
        GetComponent<Text>().text = message.ReadMessage<DisplayInstructionMessage>().instruction;
    }
}
