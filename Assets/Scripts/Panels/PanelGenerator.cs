using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PanelGenerator : MonoBehaviour {
    public const int CELLS_X = 3;
    public const int CELLS_Y = 2;

    [SerializeField]
    protected List<GameObject> panelPrefabs;

    //Vars for generator for all players
    private HashSet<string> _createdPanels = new HashSet<string>();

    //Vars for generation for a single player
    private Dictionary<InteractionPanel, GameObject> _panelsToChooseFrom = null;
    private bool[,] _isCellFilled = new bool[CELLS_X, CELLS_Y];
    private int emptyCells = 0;

    void Awake() {
        CustomMessage.registerClientHandler<CreatePanelMessage>(panelCreator);
    }

    public List<List<CreatePanelMessage>> generateAllPanelsForAllPlayers(int playerCount) {
        _createdPanels.Clear();

        List<List<CreatePanelMessage>> allPanelsForAllPlayers = new List<List<CreatePanelMessage>>();

        for (int i = 0; i < playerCount; i++) {
            allPanelsForAllPlayers.Add(generateAllPanelsForOnePlayer());
        }

        return allPanelsForAllPlayers;
    }

    private List<CreatePanelMessage> generateAllPanelsForOnePlayer() {
        _isCellFilled.fill(() => false);

        _panelsToChooseFrom = new Dictionary<InteractionPanel, GameObject>();
        foreach(GameObject obj in panelPrefabs){
            _panelsToChooseFrom[obj.GetComponentInChildren<InteractionPanel>()] = obj;
        }

        emptyCells = CELLS_X * CELLS_Y;

        List<CreatePanelMessage> allPanelsForPlayer = new List<CreatePanelMessage>();
        while (emptyCells != 0) {
            int filledCells = 0;
            CreatePanelMessage randomPanel = getRandomPanel(out filledCells);

            emptyCells -= filledCells;
            allPanelsForPlayer.Add(randomPanel);
            _createdPanels.Add(randomPanel.actionSet.panelLabel);
        }

        return allPanelsForPlayer;
    }

    private CreatePanelMessage getRandomPanel(out int filledCells) {
        InteractionPanel chosenPanel = null;
        GameObject chosenObj = null;
        int chosenX, chosenY;
        PanelActionSetBase chosenActionSet;

        filledCells = 0;

        while(true) {
            float totalWeight = 0.0f;
            foreach (InteractionPanel panel in _panelsToChooseFrom.Keys) {
                totalWeight += panel.spawnWeight;
            }

            float r = Random.value * totalWeight;
            foreach (var pair in _panelsToChooseFrom) {
                if (pair.Key.spawnWeight > r) {
                    chosenPanel = pair.Key;
                    chosenObj = pair.Value;
                    break;
                } else {
                    r -= pair.Key.spawnWeight;
                }
            }

            List<int> availableCellLocationsX = new List<int>();
            List<int> availableCellLocationsY = new List<int>();
            for (int x = 0; x < CELLS_X - chosenPanel.dimensionX + 1; x++) {
                for (int y = 0; y < CELLS_Y - chosenPanel.dimensionY + 1; y++) {

                    bool isAnyCellFilled = false;
                    for (int dx = 0; dx < chosenPanel.dimensionX; dx++) {
                        for (int dy = 0; dy < chosenPanel.dimensionY; dy++) {
                            if (_isCellFilled[x + dx, y + dy]) {
                                isAnyCellFilled = true;
                                break;
                            }
                        }

                        if (isAnyCellFilled) {
                            break;
                        }
                    }

                    if (!isAnyCellFilled) {
                        availableCellLocationsX.Add(x);
                        availableCellLocationsY.Add(y);
                    }
                }
            }

            if (availableCellLocationsX.Count == 0) {
                _panelsToChooseFrom.Remove(chosenPanel);

                if (_panelsToChooseFrom.Count == 0) {
                    throw new System.Exception("Cannot fit any more panels!");
                }
                continue;
            }

            int chosenCellIndex = Random.Range(0, availableCellLocationsX.Count);
            chosenX = availableCellLocationsX[chosenCellIndex];
            chosenY = availableCellLocationsY[chosenCellIndex];
            chosenActionSet = chosenPanel.createViableActionSet(_createdPanels);
            break;
        };

        filledCells = chosenPanel.dimensionY * chosenPanel.dimensionX;

        CreatePanelMessage creationMessage = new CreatePanelMessage(chosenActionSet, chosenX, chosenY, panelPrefabs.IndexOf(chosenObj));
        return creationMessage;
    }

    private void panelCreator(NetworkMessage message) {
        CreatePanelMessage panelMessage = message.ReadMessage<CreatePanelMessage>();

        GameObject obj = (Instantiate(panelPrefabs[panelMessage.prefabIndex], new Vector3(panelMessage.x, panelMessage.y, 0), Quaternion.identity) as GameObject);
        obj.SetActive(true);
        InteractionPanel newPanel = obj.GetComponent<InteractionPanel>();
        //TODO, use x and y to place
        newPanel.setActionSet(panelMessage.actionSet);
    }
}
