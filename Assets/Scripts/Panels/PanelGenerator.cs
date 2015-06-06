using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PanelGenerator : MonoBehaviour {
    public const float CELL_SIZE = 0.2f;
    public const float CELL_Z_OFFSET = 0.3f;
    public const int CELLS_X = 4;
    public const int CELLS_Y = 4;

    [SerializeField]
    protected List<InteractionPanel> panelPrefabs;

    //Vars for generator for all players
    private static HashSet<string> _createdPanels = new HashSet<string>();

    //Vars for generation for a single player
    private static List<InteractionPanel> _panelsToChooseFrom = null;
    private static bool[,] _isCellFilled = new bool[CELLS_X, CELLS_Y];
    private static int emptyCells = 0;

    private static PanelGenerator _instance;

    void Awake() {
        _instance = this;
        CustomMessage.registerClientHandler<CreatePanelMessage>(panelCreator);
    }

    public static List<List<CreatePanelMessage>> generateAllPanelsForAllPlayers(int playerCount) {
        _createdPanels.Clear();

        List<List<CreatePanelMessage>> allPanelsForAllPlayers = new List<List<CreatePanelMessage>>();

        for (int i = 0; i < playerCount; i++) {
            allPanelsForAllPlayers.Add(generateAllPanelsForOnePlayer());
        }

        return allPanelsForAllPlayers;
    }

    private static List<CreatePanelMessage> generateAllPanelsForOnePlayer() {
        _isCellFilled.fill(() => false);
        _panelsToChooseFrom = new List<InteractionPanel>();
        _panelsToChooseFrom.AddRange(_instance.panelPrefabs);
        emptyCells = CELLS_X * CELLS_Y;

        List<CreatePanelMessage> allPanelsForPlayer = new List<CreatePanelMessage>();
        while (emptyCells != 0) {
            allPanelsForPlayer.Add(createRandomPanel());
        }

        return allPanelsForPlayer;
    }

    private static CreatePanelMessage createRandomPanel() {
        InteractionPanel chosenPanel = null;
        int chosenX, chosenY;
        PanelActionSetBase chosenActionSet;

        while(true) {
            float totalWeight = 0.0f;
            foreach (InteractionPanel panel in _panelsToChooseFrom) {
                totalWeight += panel.spawnWeight;
            }

            float r = Random.value * totalWeight;
            foreach (InteractionPanel panel in _panelsToChooseFrom) {
                if (panel.spawnWeight > r) {
                    chosenPanel = panel;
                    break;
                } else {
                    r -= panel.spawnWeight;
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

        emptyCells -= chosenPanel.dimensionY * chosenPanel.dimensionX;
        _createdPanels.Add(chosenActionSet.panelLabel);

        for (int dx = 0; dx < chosenPanel.dimensionX; dx++) {
            for (int dy = 0; dy < chosenPanel.dimensionY; dy++) {
                _isCellFilled[chosenX + dx, chosenY + dy] = true;
            }
        }

        chosenActionSet.currentVariantIndex = Random.Range(0, chosenActionSet.getVariantCount());

        CreatePanelMessage creationMessage = new CreatePanelMessage(chosenActionSet, chosenX, chosenY, _instance.panelPrefabs.IndexOf(chosenPanel));
        return creationMessage;
    }

    private static void panelCreator(NetworkMessage message) {
        CreatePanelMessage panelMessage = message.ReadMessage<CreatePanelMessage>();

        InteractionPanel newPanel = Instantiate<InteractionPanel>(_instance.panelPrefabs[panelMessage.prefabIndex]);

        float leftMostCellCenter = (-(CELLS_X - 1) / 2.0f - 0.5f) * CELL_SIZE;
        float downMostCellCenter = (-(CELLS_Y - 1) / 2.0f - 0.5f) * CELL_SIZE;

        float panelX = (panelMessage.x + newPanel.dimensionX / 2.0f) * CELL_SIZE;
        float panelY = (panelMessage.y + newPanel.dimensionY / 2.0f) * CELL_SIZE;
        float panelZ = CELL_Z_OFFSET;

        newPanel.transform.position = new Vector3(panelX + leftMostCellCenter, panelY + downMostCellCenter, panelZ);

        newPanel.setActionSet(panelMessage.actionSet);
    }
}
