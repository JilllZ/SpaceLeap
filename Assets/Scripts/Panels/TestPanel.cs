using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TestPanel : MonoBehaviour {
    private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private char _chosenLetter;
    private KeyCode _chosenKeyCode;

    private SinglePanelActionSet _actionSet;

    void Awake() {
        _chosenLetter = ALPHABET.chooseRandom();
        int offset = _chosenLetter - 'A';
        _chosenKeyCode = (KeyCode)((int)KeyCode.A + offset);

        _actionSet = new SinglePanelActionSet("Press " + _chosenLetter);

        GetComponent<Text>().text = _chosenKeyCode.ToString();
    }

    void Start() {
        _actionSet.initialize();
    }

    void Update() {
        if (Input.GetKeyDown(_chosenKeyCode)) {
            new PanelActionMessage(_actionSet.setId).sendToServer();
        }
    }
}
