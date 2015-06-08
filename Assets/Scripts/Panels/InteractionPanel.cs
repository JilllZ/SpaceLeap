using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InteractionPanel : MonoBehaviour {
    [MinValue(0.01f)]
    public float spawnWeight;
    [MinValue(1)]
    public int dimensionX;
    [MinValue(1)]
    public int dimensionY;
    public List<GameObject> backPanelPrefabs;

    //All actions for this panel
    protected PanelActionSetBase _actionSet;

    public virtual void setActionSet(PanelActionSetBase actionSet) {
        _actionSet = actionSet;
    }

    protected virtual void Awake() {
        if (backPanelPrefabs.Count != 0) {
            GameObject backPanelPrefab = Instantiate<GameObject>(backPanelPrefabs.chooseRandom());
            backPanelPrefab.transform.localPosition = transform.position;
            backPanelPrefab.transform.localRotation = transform.rotation;
            backPanelPrefab.transform.SetParent(transform, true);
        }
    }

    public abstract PanelActionSetBase createViableActionSet(HashSet<string> existingLabels);

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(dimensionX, dimensionY, 0) * PanelGenerator.CELL_SIZE);
    }
}
