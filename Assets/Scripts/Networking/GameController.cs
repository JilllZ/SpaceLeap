using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GameController : NetworkBehaviour {
    private Dictionary<int, PanelActionSetBase> _idToPanelActionSets = new Dictionary<int, PanelActionSetBase>();

    [ServerCallback]
    void Start() {
        CustomMessage.registerHandler<PanelActionMessage>(handlePanelAction);
    }

    private void handlePanelAction(PanelActionMessage panelAction) {
        _idToPanelActionSets[panelAction.setId].currentVariantIndex = panelAction.variantIndex;
    }

}
