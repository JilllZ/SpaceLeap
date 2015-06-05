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

    //Maps the reciever of the instruction to the server instruction struct
    private Dictionary<int, ServerInstruction> _currentInstructions = new Dictionary<int, ServerInstruction>();

    [ServerCallback]
    void Start() {
        //Register the handler for the panel actions coming from the clients
        CustomMessage.registerHandler<PanelActionMessage>(handlePanelAction);

        //Register handlers for the definitions of panel action sets
        CustomMessage.registerHandler<CodePanelActionSet>(s => handleGenericPanelActionSet(s));
        CustomMessage.registerHandler<SinglePanelActionSet>(s => handleGenericPanelActionSet(s));
        CustomMessage.registerHandler<ReplacementPanelActionSet>(s => handleGenericPanelActionSet(s));
    }

    private void handleGenericPanelActionSet(PanelActionSetBase set) {
        _idToPanelActionSets[set.setId] = set;
        List<PanelActionSetBase> idList;
        if (!_connectionIdToPanelIds.TryGetValue(set.senderId, out idList)) {
            idList = new List<PanelActionSetBase>();
            _connectionIdToPanelIds[set.senderId] = idList;
        }
        idList.Add(set);
    }

    private void handlePanelAction(PanelActionMessage panelAction) {
        _idToPanelActionSets[panelAction.setId].currentVariantIndex = panelAction.variantIndex;

        var satisfiedInstruction = (from instruction in _currentInstructions.Values
                                   where instruction.panelActionSetId == panelAction.setId
                                   where instruction.panelActionVariantIndex == panelAction.variantIndex
                                   select instruction).FirstOrDefault();

        if (satisfiedInstruction != null) {
            issueNewInstruction(satisfiedInstruction.instructionReaderConnection);
        }

    }

    private void issueNewInstruction(int recieverConnectionId) {
        bool shouldPerformAsWell = Random.value > instructionPerformerBias;
        int performerConnectionId;
        if (shouldPerformAsWell) {
            performerConnectionId = recieverConnectionId;
        } else {
            performerConnectionId = (from connection in NetworkServer.connections
                                     where connection.connectionId != recieverConnectionId
                                     select connection.connectionId).chooseRandom();
        }

        List<PanelActionSetBase> panelActionSets;
        if (!_connectionIdToPanelIds.TryGetValue(performerConnectionId, out panelActionSets)) {
            throw new System.Exception("Could not find list of panel action sets for connection id " + performerConnectionId);
        }

        PanelActionSetBase randomPanelActionSet = panelActionSets.chooseRandom();
        int variantIndex = Random.Range(0, randomPanelActionSet.getVariantCount());

        ServerInstruction newInstruction = new ServerInstruction(recieverConnectionId, performerConnectionId, randomPanelActionSet.setId, variantIndex);
        _currentInstructions[recieverConnectionId] = newInstruction;

        string instructionText = randomPanelActionSet.getVariant(variantIndex);
        DisplayInstructionMessage displayInstructionMessage = new DisplayInstructionMessage(instructionText);
        displayInstructionMessage.sendToClient(recieverConnectionId);
    }
}
