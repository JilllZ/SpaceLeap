using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class PlayerController : NetworkBehaviour {
    private static PlayerController _localPlayer;

    public static PlayerController localPlayer {
        get {
            return _localPlayer;
        }
    }

    [ClientCallback]
    void Start() {
        if (isLocalPlayer) {
            _localPlayer = this;
        }
    }

    [ClientCallback]
    void Update() {
        if(!isLocalPlayer){
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            new TestMessage().sendToServer();
        }
    }
}
