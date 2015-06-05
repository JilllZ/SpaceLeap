using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LMWidgets;

public class ToggleButtonPanel : InteractionObject, IDataBoundWidget<ButtonToggleBase, bool> {
    public Text label;
    public ButtonToggleBase button;

    void Start() {

    }
}
