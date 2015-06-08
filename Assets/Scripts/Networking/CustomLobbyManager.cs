using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CustomLobbyManager : NetworkLobbyManager {
    private static NetworkClient _myClient;
    private static int _startedClients = 0;

    private static CustomLobbyManager _instance;

    public static CustomLobbyManager instance {
        get {
            return _instance;
        }
    }

    public static NetworkClient myClient {
        get {
            return _myClient;
        }
    }

    public static bool allClientsStarted {
        get {
            return _startedClients == allConnections.Count();
        }
    }

    public static IEnumerable<NetworkConnection> allConnections {
        get {
            return NetworkServer.localConnections.Concat(NetworkServer.connections).Where(c => c != null);
        }
    }

    public override void OnLobbyStartClient(NetworkClient client) {
        base.OnLobbyStartClient(client);
        _myClient = client;
    }

    public override void OnServerSceneChanged(string sceneName) {
        base.OnServerSceneChanged(sceneName);
        if (sceneName == lobbyScene) {
            _startedClients = 0;
        }
    }

    
    void Start() {
        _instance = this;
        CustomMessage.registerServerHandler<ClientStartGame>(onClientStartGame);
    }

    private void onClientStartGame(NetworkMessage message) {
        _startedClients++;
    }
}
