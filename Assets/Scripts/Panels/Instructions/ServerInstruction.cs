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

    public override bool Equals(object obj) {
        ServerInstruction other = obj as ServerInstruction;
        if (other == null) {
            return false;
        }

        return other.instructionPerformerConnection == instructionPerformerConnection &&
               other.instructionReaderConnection == instructionReaderConnection &&
               other.panelActionSetId == panelActionSetId &&
               other.panelActionVariantIndex == panelActionVariantIndex;
    }

    public override int GetHashCode() {
        int hash = 17;
        hash = hash * 23 + instructionReaderConnection;
        hash = hash * 23 + instructionPerformerConnection;
        hash = hash * 23 + panelActionSetId;
        hash = hash * 23 + panelActionVariantIndex;
        return hash;
    }

    public bool conflictsWith(ServerInstruction other) {
        return other.instructionPerformerConnection == instructionPerformerConnection &&
               other.instructionReaderConnection != instructionReaderConnection &&
               other.panelActionSetId == panelActionSetId;
    }
}