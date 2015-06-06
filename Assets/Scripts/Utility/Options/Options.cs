using UnityEngine;
using Ovr;
using System.Collections;

public class Options : MonoBehaviour {
    public static KeyCode quit = KeyCode.Escape;
    public static KeyCode recenter = KeyCode.Space;
    public static KeyCode timewarp = KeyCode.T;
    public static KeyCode reload = KeyCode.Backspace;
	
	void Update () {
        if (Input.GetKeyDown(quit)) {
            Application.Quit();
        }

        if (Input.GetKeyDown(recenter)) {
            OVRManager.display.RecenterPose();
        }

        if (Input.GetKeyDown(timewarp)) {
            OVRManager.display.timeWarp = !OVRManager.display.timeWarp;
        }

        if (Input.GetKeyDown(reload)) {
            Application.LoadLevel(Application.loadedLevel);
        }
	}
}
