using UnityEngine;
using Leap;
using System.Collections;

public class HandUtil {

    public static float grabAmount(Hand hand) {
        return hand.GrabStrength;
    }

    public static float grabDeltaAmount(Hand hand, int frameDelta) {
        Frame pastFrame = HandControllerUtil.handController.GetLeapController().Frame(frameDelta);
        if (pastFrame == null || !pastFrame.IsValid) {
            return 0;
        }

        Hand prevHand = pastFrame.Hand(hand.Id);
        if (prevHand == null || !pastFrame.IsValid) {
            return 0;
        }

        return (grabAmount(hand) - grabAmount(prevHand)) / (HandControllerUtil.handController.GetFrame().Timestamp - pastFrame.Timestamp);
    }

    public static float grabHeuristic(Hand hand, bool useVel) {
        float ret = hand.GrabStrength;

        if (useVel && hand.PalmVelocity.ToUnityScaled().sqrMagnitude > 0.09f) {
            ret = 0.25f;
        }

        return ret;
    }

    public static Vector3 smoothedPalm(Hand hand) {
        Vector3 palmPos = 0.5f * HandControllerUtil.toUnitySpace(hand.PalmPosition);
        foreach (Finger finger in hand.Fingers) {
            palmPos += 0.1f * HandControllerUtil.toUnitySpace(finger.TipPosition);
        }
        return palmPos;
    }

    public static float reachDistance(Hand hand, Vector3 center) {
        float distance = Vector3.Distance(HandControllerUtil.toUnitySpace(hand.PalmPosition), center);

        foreach (Finger finger in hand.Fingers) {
            distance = Mathf.Max(distance, Vector3.Distance(HandControllerUtil.toUnitySpace(finger.TipPosition), center));
        }

        return distance;
    }

    public static float reachDistance(Hand hand) {
        return reachDistance(hand, HandControllerUtil.head());
    }
}
