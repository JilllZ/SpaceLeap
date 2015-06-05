using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CustomLobbyManager : NetworkLobbyManager {
    private static NetworkClient _myClient;

    public static NetworkClient myClient {
        get {
            return _myClient;
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
}
