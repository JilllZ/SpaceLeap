using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using System.Collections;
using System.Collections.Generic;

public class LobbyGUI : NetworkMatch {
    public Button hostButton;
    public Button joinButton;
    public Button readyButton;
    public Text playerList;

    private List<MatchDesc> _matchList = null;
    private bool _matchCreated = false;

    void Awake() {
        SetProgramAppID((AppID)1401);
    }

    void Start() {
        StartCoroutine(updatePlayerList());
    }

    IEnumerator updatePlayerList() {
        while (true) {
            yield return new WaitForSeconds(0.5f);

            playerList.text = "";
            NetworkLobbyPlayer[] lobbyPlayers = FindObjectsOfType<NetworkLobbyPlayer>();
            int i = 1;
            foreach (NetworkLobbyPlayer player in lobbyPlayers) {
                playerList.text += "Player " + i + " : " + (player.readyToBegin ? "Ready" : "Not Ready") + "\n";
            }
        }
    }

    public void hostRoom() {
        joinButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        CreateMatchRequest createRequest = new CreateMatchRequest();
        createRequest.name = "Room";
        createRequest.size = 4;
        createRequest.advertise = true;
        createRequest.password = "";

        CreateMatch(createRequest, onMatchCreate);
    }

    public void searchAndJoin() {
        joinButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        ListMatches(0, 1, "", onMatchList);
    }

    public void pressReady() {
        NetworkLobbyPlayer[] lobbyPlayers = FindObjectsOfType<NetworkLobbyPlayer>();
        foreach (NetworkLobbyPlayer player in lobbyPlayers) {
            if (player.isLocalPlayer) {
                player.SendReadyToBeginMessage();
            }
        }
    }

    private void onMatchCreate(CreateMatchResponse matchResponse) {
        if (matchResponse.success) {
            Debug.Log("Creates match");
            Utility.SetAccessTokenForNetwork(matchResponse.networkId, new UnityEngine.Networking.Types.NetworkAccessToken(matchResponse.accessTokenString));
            FindObjectOfType<CustomLobbyManager>().StartHost(new MatchInfo(matchResponse));
            _matchCreated = true;
            readyButton.gameObject.SetActive(true);
            playerList.transform.parent.gameObject.SetActive(true);
        } else {
            joinButton.gameObject.SetActive(true);
            hostButton.gameObject.SetActive(true);
            Debug.LogError("Failed to create match");
        }
    }

    

    private void onMatchList(ListMatchResponse listResponse) {
        if (listResponse.success && listResponse.matches != null && listResponse.matches.Count == 1) {
            JoinMatch(listResponse.matches[0].networkId, "", onMatchJoined);
        } else {
            joinButton.gameObject.SetActive(true);
            hostButton.gameObject.SetActive(true);
            Debug.LogError("Failed to join match");
        }
    }

    private void onMatchJoined(JoinMatchResponse joinResponse) {
        if (joinResponse.success) {
            Debug.Log("Joining match...");
            if (_matchCreated) {
                Debug.LogWarning("Match already set up.... aborting");
                return;
            }
            Utility.SetAccessTokenForNetwork(joinResponse.networkId, new UnityEngine.Networking.Types.NetworkAccessToken(joinResponse.accessTokenString));
            FindObjectOfType<CustomLobbyManager>().StartClient(new MatchInfo(joinResponse));
        } else {
            joinButton.gameObject.SetActive(true);
            hostButton.gameObject.SetActive(true);
            Debug.LogError("Failed to join match");
        }
    }

    private void onConnected(NetworkMessage message) {
        Debug.Log("Connected to match");
        readyButton.gameObject.SetActive(true);
        playerList.transform.parent.gameObject.SetActive(true);
    }
}
