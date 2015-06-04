using UnityEngine;
using Leap;
using System.Collections;

public class HandInfo {
    private static HandController _handController;
    private static Controller leapController;

    public static HandController handController {
        get {
            if (_handController == null) {
                _handController = GameObject.FindObjectOfType<HandController>();
                leapController = _handController.GetLeapController();
            }
            return _handController;
        }
    }

    public static Hand getHand(int id){
        return handController.GetFrame().Hand(id);
    }

    public static float getPinchDistance(int handId) {
        return getPinchDistance(getHand(handId));
    }

    public static float getPinchDistance(Hand hand) {
        Finger thumb = hand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0];
        Finger index = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];
        return (thumb.TipPosition - index.TipPosition).MagnitudeSquared;
    }

    public static Vector3 getPinchPosition(int handId) {
        return getPinchPosition(getHand(handId));
    }

    public static Vector3 getPinchPosition(Hand hand) {
        Finger thumb = hand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0];
        Finger index = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0];
        Vector pinchPos = (thumb.TipPosition + index.TipPosition) / 2.0f;
        return handController.transform.TransformPoint(pinchPos.ToUnityScaled());
    }

    public static Vector3 unityPos(Vector vector) {
        return handController.transform.TransformPoint(vector.ToUnityScaled());
    }

    public static Vector3 unityDir(Vector vector) {
        return handController.transform.TransformDirection(vector.ToUnity());
    }

    public static float unityLength(float length) {
        return handController.transform.TransformDirection(Vector3.right * length).magnitude;
    }

    public static float getGrabHeuristic(Hand hand, GameObject obj) {
        float grabProbability = 0.0f;

        //If palm is facing away from the object, no chance of grabbing
        Vector3 palmNormal = handController.transform.TransformDirection(hand.PalmNormal.ToUnity());
        Vector3 palmPos = handController.transform.TransformPoint(hand.PalmPosition.ToUnityScaled());
        if (Vector3.Dot(palmNormal, obj.transform.position - palmPos) < 0) {
            return 0;
        }

        float proximityFactor = 0.0f;
        foreach (Finger finger in hand.Fingers) {
            Vector3 point = handController.transform.TransformPoint(finger.JointPosition(Leap.Finger.FingerJoint.JOINT_TIP).ToUnityScaled());
            Vector3 pointOn = ColliderUtil.closestPointOnSurfaces(obj, point);
            float distance = Vector3.Distance(point, pointOn);
            if (ColliderUtil.isInsideColliders(obj, point)) {
                distance = 0.0f;
            }

            //if any finger is <releaseRadius> meters away, negative infinity, no way that hand is close enough
            float thresh = Mathf.Clamp01(1.0f - distance / 0.15f);
            proximityFactor += 2.0f - 1.0f / thresh;
        }

        proximityFactor /= hand.Fingers.Count;
        //Debug.Log(proximityFactor);

        grabProbability += proximityFactor;

        //The more the hand is opening, the more grabbable
        grabProbability += getHandClosingSpeed(hand) * 10.0f;

        //If the hand is mostly open, the probability of a grab goes down
        grabProbability -= Mathf.Pow(1.0f - getHandClosedPercent(hand), 4.0f);

        if (hand.PalmVelocity.ToUnityScaled().sqrMagnitude > 0.09f) {
            grabProbability = 0.7f;
        }

        grabProbability = 0.7f + (grabProbability - 0.7f) / (hand.PalmVelocity.ToUnityScaled().sqrMagnitude * 3.0f + 1);

        grabProbability = 0.7f + (grabProbability - 0.7f) / (getHandRotationAmount(hand) * 1 + 1.0f);

		if (grabProbability > 0 && grabProbability < 10.0f) {
			DebugGraph.Log("Grabbing", grabProbability, grabProbability > 0.7f ? Color.green : Color.red);
		}

        return grabProbability;
    }

    public static float getHandRotationAmount(Hand hand) {
        Leap.Vector normal = hand.PalmNormal;
        Frame prevFrame = leapController.Frame(3);
        if (prevFrame != null) {
            Hand prevHand = prevFrame.Hand(hand.Id);
            if (prevFrame != null && prevHand.IsValid) {
                return 0.5f + normal.Dot(prevHand.PalmNormal) / 2.0f;
            }
        }
        return 0.0f;
    }

    public static float getHandClosingSpeed(Hand hand) {
        Frame prevFrame = leapController.Frame(10);
        if (prevFrame != null) {
            Hand prevHand = prevFrame.Hand(hand.Id);
            if (prevHand != null) {
                return getHandClosedPercent(hand) - getHandClosedPercent(prevHand);
            }
        }
        return 0.0f;
    }

    public static float getHandClosedPercent(Hand hand) {
        float prob = 0.0f;
        Leap.Vector palmNormal = hand.PalmNormal;
        foreach (Finger finger in hand.Fingers) {
            prob += palmNormal.Dot(finger.Direction);
        }
        prob /= hand.Fingers.Count;
        return prob;
    }
}
