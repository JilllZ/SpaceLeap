using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TestTogglePanel : InteractionPanel {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int VARIANT_OFF = 0;
    private const int VARIANT_ON = 1;

    public Toggle toggleButton;
    public Text label;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        char randomChar = ALPHABET.chooseRandom(c => !existingLabels.Contains(c.ToString()));
        return new ReplacementPanelActionSet(randomChar.ToString(), "Toggle " + randomChar + " #", "Off", "On");
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);
        label.text = actionSet.panelLabel;
        toggleButton.isOn = actionSet.currentVariantIndex == VARIANT_ON;
    }

    public void OnToggleChange() {
        new PanelActionMessage(_actionSet.setId, toggleButton.isOn ? VARIANT_ON : VARIANT_OFF).sendToServer();
    }
}
