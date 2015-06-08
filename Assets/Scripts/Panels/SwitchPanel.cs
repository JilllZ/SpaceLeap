using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SwitchPanel : InteractionPanel {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public Renderer lightRenderer;
    public Color onColor;
    public Color offColor;
    public Transform switchBody;
    public Transform switchGraphic;
    public Text label;

    public float range = 45;

    private HingeJoint _joint;
    private bool _prevOnState = false;

    public override PanelActionSetBase createViableActionSet(HashSet<string> existingLabels) {
        string[] toggleOpts = {
          "Vector Control",
          "Husband",
          "C++11",
          "Nelder-Mead",
          "Optical Flow",
          "Tachyon Particles",
          "Pineapple Cutter",
          "Radio Cylon",
          "Light Cylon",
          "Nightcrawler",
          "Mantis",
          "Meadowhawk",
          "Ab Wheel",
          "Headphones",
          "Dragonfly",
          "Bison",
          "Team-Building",
          "Upper Management",
          "Actuator",
          "Caveat",
          "Galaxy Note 4",
          "Urgency",
          "Phone Tracking",
          "Accounting",
          "Servoactuator Anti-Skid Valves",
          "Japanese Selvedge Jeans"
        };
        string buttonName = toggleOpts.chooseRandom(str => !existingLabels.Contains(str));
        string actionText = "Turn " + buttonName + " #";
        return new ReplacementPanelActionSet(buttonName, actionText, "On", "Off");
    }

    public override void setActionSet(PanelActionSetBase actionSet) {
        base.setActionSet(actionSet);
        _prevOnState = actionSet.currentVariantIndex == 0;
        switchBody.transform.localEulerAngles = new Vector3(_prevOnState ? 1 : -1, 0, 0);
        label.text = _actionSet.panelLabel;
    }

    protected override void Awake() {
        base.Awake();
        _joint = switchBody.GetComponent<HingeJoint>();
    }

    void Update() {
        switchGraphic.localEulerAngles = new Vector3(switchBody.localEulerAngles.x, 0, 0);

        lightRenderer.material.color = isSwitchOn() ? onColor : offColor;
        if (isSwitchOn() != _prevOnState) {
            _prevOnState = isSwitchOn();

            JointSpring s = _joint.spring;
            s.targetPosition = _prevOnState ? range : -range;
            _joint.spring = s;

            new PanelActionMessage(_actionSet.setId, _prevOnState ? 0 : 1).sendToServer();     
        }
    }

    private bool isSwitchOn() {
        return switchBody.transform.localEulerAngles.x > 180;
    }
}
