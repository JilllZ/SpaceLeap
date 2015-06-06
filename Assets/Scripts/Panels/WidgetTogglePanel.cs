using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LMWidgets;

public class WidgetTogglePanel : InteractionPanel {
    private const int VARIANT_ON = 0;
    private const int VARIANT_OFF = 1;

    public Text label;
    public ButtonToggleBase button;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        string[] toggleOpts = {
          "Emergency Nose Landing",
          "Main Screen",
          "Hydraulic Power Drive Unit",
          "Leap",
          "Autowiring",
          "Tracking"
        };
        string buttonName = toggleOpts.chooseRandom(str => !existingLabels.Contains(str));
        return new ReplacementPanelActionSet(buttonName, "Turn " + buttonName + " #", "On", "Off");
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
