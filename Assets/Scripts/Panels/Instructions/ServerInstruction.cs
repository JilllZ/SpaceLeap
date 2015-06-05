using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ServerInstruction {
    public readonly int instructionReaderConnection;       //Network id of the player to recieve the instruction
    public readonly int instructionPerformerConnection;    //Network id of the player who has the panel the instruction references
    public readonly int panelActionSetId;                  //The id of the panel action set the instruction references
    public readonly int panelActionVariantIndex;           //The index of the action variant the instruction references

    public ServerInstruction(int instructionReaderConnection,
                             int instructionPerformerConnection,
                             int panelActionSetId,
                             int panelActionVariantIndex) {
        this.instructionReaderConnection = instructionReaderConnection;
        this.instructionPerformerConnection = instructionPerformerConnection;
        this.panelActionSetId = panelActionSetId;
        this.panelActionVariantIndex = panelActionVariantIndex;
    }
}