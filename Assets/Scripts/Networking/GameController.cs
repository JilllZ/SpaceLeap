using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameController : NetworkBehaviour {
    [SyncVar]
    public bool displayGui = false;

    public void toggleBox() {
        displayGui = !displayGui;
    }

    [ClientCallback]
    void OnGUI() {
        if (displayGui) {
            GUILayout.Box("OMG ITS A FLASHING BOX");
        }
    }
}
