using UnityEngine;
using Leap;
using System.Collections;

public class HandControllerUtil {
    private static HandController _handController;

    public static HandController handController {
        get {
            if (_handController == null) {
                _handController = GameObject.FindObjectOfType<HandController>();
            }
            return _handController;
        }
    }

    public static Frame frame() {
        return handController.GetFrame();
    }

    public static Vector3 toUnitySpace(Vector pos) {
        return handController.transform.TransformPoint(pos.ToUnityScaled());
    }

    public static Vector3 toUnitySpace(Vector3 pos) {
        return handController.transform.TransformPoint(pos);
    }

    public static Vector3 toUnityDir(Vector dir) {
        return handController.transform.TransformDirection(dir.ToUnity());
    }

    public static Vector3 head() {
        return handController.transform.position;
    }
}
