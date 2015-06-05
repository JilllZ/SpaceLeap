using UnityEngine;
using System.Collections;

/* Message is sent from client to server whenever the user performs an 
 * action on a panel.  Needs to be sent at all time so that the server 
 * does not choose an instruction that has already been followed
 */
public class PanelActionMessage : CustomMessage {
    public int setId;
    public int variantIndex;

    public PanelActionMessage() { }

    public PanelActionMessage(int setId, int variantNumber) {
        this.setId = setId;
        this.variantIndex = variantNumber;
    }
}
