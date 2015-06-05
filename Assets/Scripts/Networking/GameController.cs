using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameController : NetworkBehaviour {
    [SyncVar]
    public bool displayGui = false;

    [ServerCallback]
    void Start() {
        CustomMessage.registerHandler<TestMessage>(handleTestMessage);
    }

    private void handleTestMessage(TestMessage message) {
        displayGui = !displayGui;
    }

    [ClientCallback]
    void OnGUI() {
        if (displayGui) {
            GUILayout.Box("OMG ITS A FLASHING BOX");
        }
    }
}
