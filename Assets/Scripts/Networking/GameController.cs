using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameController : NetworkBehaviour {
    [Range(0, 1)]
    [Tooltip ("At 0, an instruction will always be issued to the player who can perform the instruction.  At 1, an instruction will never be issued to the player that can perform the instruction")]
    public float instructionPerformerBias = 0.5f;

    private Dictionary<int, PanelActionSetBase> _idToPanelActionSets = new Dictionary<int, PanelActionSetBase>();
    private Dictionary<int, List<PanelActionSetBase>> _connectionIdToPanelIds = new Dictionary<int, List<PanelActionSetBase>>();

    //List of all instructions currently being dispatched
    private List<ServerInstruction> _currentInstructions = new List<ServerInstruction>();
    
    [ServerCallback]
    void Awake() {
        //Register the handler for the panel actions coming from the clients
        CustomMessage.registerServerHandler<PanelActionMessage>(handlePanelAction);
        CustomMessage.registerServerHandler<InstructionMissed>(handleMissedInstruction);
    }

    void Start() {
        StartCoroutine(waitForAllClientsToStart());
    }

    private void handleMissedInstruction(NetworkMessage message) {
        issueNewInstruction(message.conn.connectionId);
        new ProgressMessage(-1).sendToAllClients();
    }

    private IEnumerator waitForAllClientsToStart() {
        while (!CustomLobbyManager.allClientsStarted) {
            yield return null;
        }

        int playerCount = CustomLobbyManager.allConnections.Count();

        List<List<CreatePanelMessage>> allPanels = PanelGenerator.generateAllPanelsForAllPlayers(playerCount);

        int index = 0;
        foreach (NetworkConnection connection in CustomLobbyManager.allConnections) {
            foreach (CreatePanelMessage createPanelMessage in allPanels[index]) {
                PanelActionSetBase actionSet = createPanelMessage.actionSet;

                _idToPanelActionSets[actionSet.setId] = actionSet;
                List<PanelActionSetBase> idList;
                if (!_connectionIdToPanelIds.TryGetValue(connection.connectionId, out idList)) {
                    idList = new List<PanelActionSetBase>();
                    _connectionIdToPanelIds[connection.connectionId] = idList;
                }
                idList.Add(actionSet);

                createPanelMessage.sendToClient(connection);
            }
            index++;
        }

        foreach (var connection in CustomLobbyManager.allConnections) {
            issueNewInstruction(connection.connectionId);
        }
    }

    private void handlePanelAction(NetworkMessage message) {
        PanelActionMessage panelAction = message.ReadMessage<PanelActionMessage>();

        _idToPanelActionSets[panelAction.setId].currentVariantIndex = panelAction.variantIndex;

        var satisfiedInstruction = (from instruction in _currentInstructions
                                   where instruction.panelActionSetId == panelAction.setId
                                   where instruction.panelActionVariantIndex == panelAction.variantIndex
                                   select instruction).FirstOrDefault();

        if (satisfiedInstruction != null) {
            issueNewInstruction(satisfiedInstruction.instructionReaderConnection);
            _currentInstructions.Remove(satisfiedInstruction);
            new ProgressMessage(1).sendToAllClients();
        }

    }

    private void issueNewInstruction(int recieverConnectionId) {
        bool shouldPerformAsWell;

        if (CustomLobbyManager.allConnections.Count() == 1) {
            shouldPerformAsWell = true;
        }else{
            shouldPerformAsWell = Random.value > instructionPerformerBias;
        }

        int performerConnectionId;
        if (shouldPerformAsWell) {
            performerConnectionId = recieverConnectionId;
        } else {
            performerConnectionId = (from connection in CustomLobbyManager.allConnections
                                     where connection.connectionId != recieverConnectionId
                                     select connection.connectionId).chooseRandom();
        }

        //Grab the list of panels for the performer
        List<PanelActionSetBase> panelActionSets;
        if (!_connectionIdToPanelIds.TryGetValue(performerConnectionId, out panelActionSets)) {
            throw new System.Exception("Could not find list of panel action sets for connection id " + performerConnectionId);
        }

        PanelActionSetBase randomPanelActionSet;
        int randomVariant;
        ServerInstruction newInstruction;
        do {
            //Choose a random panel for the performer
            randomPanelActionSet = panelActionSets.chooseRandom();

            //Choose a variant that is not currently active on the panel
            do {
                randomVariant = Random.Range(0, randomPanelActionSet.getVariantCount());
            } while (randomPanelActionSet.currentVariantIndex == randomVariant);

            newInstruction = new ServerInstruction(recieverConnectionId, performerConnectionId, randomPanelActionSet.setId, randomVariant);
        } while (_currentInstructions.Contains(newInstruction) || _currentInstructions.Any(s => s.conflictsWith(newInstruction)));
        
        _currentInstructions.Add(newInstruction);

        string instructionText = randomPanelActionSet.getVariant(randomVariant);

        DisplayInstructionMessage displayInstructionMessage = new DisplayInstructionMessage(instructionText);
        displayInstructionMessage.sendToClient(recieverConnectionId);
    }
}
