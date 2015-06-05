using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameController : NetworkBehaviour {
    [SyncVar]
    public bool displayGui = false;

    [ServerCallback]
    void Start() {
        NetworkServer.RegisterHandler(TestMessage.ID, handleTestMessage);
    }

    private void handleTestMessage(NetworkMessage message) {
        displayGui = !displayGui;
    }

    [ClientCallback]
    void OnGUI() {
        if (displayGui) {
            GUILayout.Box("OMG ITS A FLASHING BOX");
        }
    }
}
