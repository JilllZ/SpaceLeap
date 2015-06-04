using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour {

    [Command]
    void CmdToggleBox() {
        FindObjectOfType<GameController>().toggleBox();
    }

    [ClientCallback]
    void Update() {
        if(!isLocalPlayer){
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            CmdToggleBox();
        }
    }
}
