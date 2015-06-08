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
    public ButtonDemoToggle button;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        if (Random.value > 0.5f) {
            return getPushSet(existingLabels);
        } else {
            return getToggleSet(existingLabels);
        }
    }

    private PanelActionSetBase getToggleSet(HashSet<string> existingLabels) {
        string[] toggleOpts = {
          "Emergency Nose Landing",
          "Main Screen",
          "Hydraulic Power Drive Unit",
          "Leap",
          "Autowiring",
          "Tracking"
        };
        // FIXME: with less than three items, will fail and say awaiting instructions
        string buttonName = toggleOpts.chooseRandom(str => !existingLabels.Contains(str));
        string actionText;
        if (string.Compare(buttonName, "Main Screen") == 0) {
            actionText = "Main Screen Turn #";
        } else {
            actionText = "Turn " + buttonName + " #";
        }

        return new ReplacementPanelActionSet(buttonName, actionText, "On", "Off");
    }

    private PanelActionSetBase getPushSet(HashSet<string> existingLabels) {
        char randomChar = ALPHABET.chooseRandom(c => !existingLabels.Contains(c.ToString()));
        return new SinglePanelActionSet(randomChar.ToString(), "Push " + randomChar);
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);

        if (actionSet.getVariantCount() == 1) {
            button.ToggleState = false;
            button.BotGraphicsOffColor = button.BotGraphicsOnColor;
            button.MidGraphicsOffColor = button.MidGraphicsOnColor;
        } else {
            button.ToggleState = actionSet.currentVariantIndex == VARIANT_ON;
        }
        
        button.onToggle += onToggle;
        label.text = _actionSet.panelLabel;
    }

    private void onToggle(bool newState) {
        if (_actionSet.getVariantCount() == 1) {
            new PanelActionMessage(_actionSet.setId, 0).sendToServer();
        } else {
            new PanelActionMessage(_actionSet.setId, newState ? VARIANT_ON : VARIANT_OFF).sendToServer();
        }
    }
}
