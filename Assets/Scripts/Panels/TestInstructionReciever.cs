using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class TestInstructionReciever : MonoBehaviour {
    public float instructionTime = 8.0f;
    public Slider slider;

    void Awake() {
        CustomMessage.registerClientHandler<DisplayInstructionMessage>(instructionHandler);
        slider.value = 1.0f;
        enabled = false;
    }

    void Update() {
        slider.value = Mathf.MoveTowards(slider.value, 0.0f, Time.deltaTime / instructionTime);
        if (slider.value <= 0.0f) {
            new InstructionMissed().sendToServer();
            enabled = false;
        }
    }

    private void instructionHandler(NetworkMessage message) {
        DisplayInstructionMessage mm = message.ReadMessage<DisplayInstructionMessage>();
        GetComponent<Text>().text = mm.instruction;

        if(mm.startTimer){
            enabled = true;
            slider.value = 1.0f;
        }
    }

    public void disable() {
        slider.value = 1.0f;
        enabled = false;
    }
}
