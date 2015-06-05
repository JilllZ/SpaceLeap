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
        } else {
            return;
        }

        new ClientStartGame().sendToServer();
    }

    [ClientCallback]
    void Update() {
        if(!isLocalPlayer){
            return;
        }

        //Do player stuff
    }
}
