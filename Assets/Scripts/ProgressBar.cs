using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ProgressBar : MonoBehaviour {
    public int maxProgress = 20;
    public int startProgress = 5;

    private int _progress;

    void Awake() {
        _progress = startProgress;
        CustomMessage.registerClientHandler<ProgressMessage>(onProgress);

        GetComponent<Slider>().value = _progress / (float)maxProgress;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.W)) {
            new WinMessage().sendToServer();
            FindObjectOfType<TestInstructionReciever>().disable();
            foreach (MoveDownUp d in FindObjectsOfType<MoveDownUp>()) {
                d.moveUp();
            }
        }
    }

    void onProgress(NetworkMessage message) {
        ProgressMessage pMessage = message.ReadMessage<ProgressMessage>();

        _progress += pMessage.progress;
        GetComponent<Slider>().value = _progress / (float)maxProgress;

        if (_progress == maxProgress) {
            new WinMessage().sendToServer();
            FindObjectOfType<TestInstructionReciever>().disable();
            foreach(MoveDownUp d in FindObjectsOfType<MoveDownUp>()){
                d.moveUp();
            }
            _progress = startProgress;
        }

        if (_progress == 0) {
            new LossMessage().sendToServer();
            FindObjectOfType<TestInstructionReciever>().disable();
            foreach (MoveDownUp d in FindObjectsOfType<MoveDownUp>()) {
                d.moveUp();
            }
        }
    }
}
