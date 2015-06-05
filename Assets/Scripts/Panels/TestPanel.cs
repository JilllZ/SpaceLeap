using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestPanel : InteractionPanel {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private KeyCode _chosenKeyCode;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        char randomChar = ALPHABET.chooseRandom(c => !existingLabels.Contains(c.ToString()));
        return new SinglePanelActionSet(randomChar.ToString(), "Press " + randomChar);
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);
        _chosenKeyCode = (KeyCode)((int)KeyCode.A + (actionSet.panelLabel[0] - 'A'));
        GetComponentInChildren<Text>().text = actionSet.panelLabel;
    }

    void Update() {
        if (Input.GetKeyDown(_chosenKeyCode)) {
            new PanelActionMessage(_actionSet.setId).sendToServer();
        }
    }
}
