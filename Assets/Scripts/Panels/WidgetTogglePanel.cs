using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LMWidgets;

public class WidgetTogglePanel : InteractionPanel {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int VARIANT_ON = 0;
    private const int VARIANT_OFF = 1;

    public Text label;
    public ButtonToggleBase button;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        char randomChar = ALPHABET.chooseRandom(c => !existingLabels.Contains(c.ToString()));
        return new ReplacementPanelActionSet(randomChar.ToString(), "Turn " + randomChar + " #", "On", "Off");
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);
        button.ToggleState = actionSet.currentVariantIndex == VARIANT_ON;
        button.onToggle += onToggle;
        label.text = _actionSet.panelLabel;
    }

    private void onToggle(bool newState) {
        new PanelActionMessage(_actionSet.setId, newState ? VARIANT_ON : VARIANT_OFF).sendToServer();
    }
}
