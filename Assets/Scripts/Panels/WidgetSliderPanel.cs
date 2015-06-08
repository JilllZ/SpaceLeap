using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;   
using LMWidgets;    

public class WidgetSliderPanel : InteractionPanel {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    [MinMax(3, 10)]
    public Vector2 dotRange;

    public Text label;
    public Text sliderDisplay;
    public SliderDemo slider;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        string[] sliderOpts = {
          "Flux Capacitor",
          "Phaser",
          "Tazer",
          "Warp",
          "Power Saving",
          "OKR Score",
          "Sprint Points",
          "CPU Usage",
          "Strobe Interval",
          "Epsilon",
          "Skeleton Separation",
          "Latency"
        };
        string sliderName = sliderOpts.chooseRandom(str => !existingLabels.Contains(str));
        string actionText = "Set " + sliderName + " To #";

        string[] variants = new string[Mathf.RoundToInt(Random.Range(dotRange.x, dotRange.y))];
        variants.fill(i => (i+1).ToString());

        return new ReplacementPanelActionSet(sliderName, actionText, variants);
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);
        slider.numberOfDots = actionSet.getVariantCount();
        slider.SetWidgetValue(_actionSet.currentVariantIndex / (_actionSet.getVariantCount() - 1.0f));
        label.text = _actionSet.panelLabel;
        slider.EndHandler += onRelease;
    }

    private void onRelease(object sender, EventArg<float> arg){
        new PanelActionMessage(_actionSet.setId, getCurrentVariant()).sendToServer();
    }

    void Update() {
        sliderDisplay.text = (_actionSet as ReplacementPanelActionSet).getReplacement(getCurrentVariant());
    }

    private int getCurrentVariant() {
        int variantNum = Mathf.RoundToInt(slider.GetSliderFraction() * (_actionSet.getVariantCount() - 1));
        return variantNum;
    }
}

