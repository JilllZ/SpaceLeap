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

    void onProgress(NetworkMessage message) {
        ProgressMessage pMessage = message.ReadMessage<ProgressMessage>();

        _progress += pMessage.progress;
        GetComponent<Slider>().value = _progress / (float)maxProgress;
    }
}
