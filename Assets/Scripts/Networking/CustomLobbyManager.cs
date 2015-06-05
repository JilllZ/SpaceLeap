using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CustomLobbyManager : NetworkLobbyManager {
    private static NetworkClient _myClient;

    public static NetworkClient myClient {
        get {
            return _myClient;
        }
    }

    public override void OnLobbyStartClient(NetworkClient client) {
        base.OnLobbyStartClient(client);
        _myClient = client;
    }

    public override void OnStartServer() {
        base.OnStartServer();
    }
}
