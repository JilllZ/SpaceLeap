﻿using UnityEngine;
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

    private IEnumerable<NetworkConnection> allConnections {
        get {
            return NetworkServer.localConnections.Concat(NetworkServer.connections).Where(c => c != null);
        }
    }

    [ServerCallback]
    void Awake() {
        //Register the handler for the panel actions coming from the clients
        CustomMessage.registerServerHandler<PanelActionMessage>(handlePanelAction);

        //Register handlers for the definitions of panel action sets
        CustomMessage.registerServerHandler<CodePanelActionSet>(s => handleGenericPanelActionSet<CodePanelActionSet>(s));
        CustomMessage.registerServerHandler<SinglePanelActionSet>(s => handleGenericPanelActionSet<SinglePanelActionSet>(s));
        CustomMessage.registerServerHandler<ReplacementPanelActionSet>(s => handleGenericPanelActionSet<ReplacementPanelActionSet>(s));
    }

    [ServerCallback]
    IEnumerator Start() {
        yield return new WaitForSeconds(1.0f);
        foreach (var connection in allConnections) {
            issueNewInstruction(connection.connectionId);
        }
    }

    private void handleGenericPanelActionSet<T>(NetworkMessage message) where T : PanelActionSetBase, new(){
        T set = message.ReadMessage<T>();

        _idToPanelActionSets[set.setId] = set;
        List<PanelActionSetBase> idList;
        if (!_connectionIdToPanelIds.TryGetValue(message.conn.connectionId, out idList)) {
            idList = new List<PanelActionSetBase>();
            _connectionIdToPanelIds[message.conn.connectionId] = idList;
        }
        idList.Add(set);
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
        }

    }

    private void issueNewInstruction(int recieverConnectionId) {
        bool shouldPerformAsWell;
        
        if(allConnections.Count() == 1){
            shouldPerformAsWell = true;
        }else{
            shouldPerformAsWell = Random.value > instructionPerformerBias;
        }

        int performerConnectionId;
        if (shouldPerformAsWell) {
            performerConnectionId = recieverConnectionId;
        } else {
            performerConnectionId = (from connection in allConnections
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