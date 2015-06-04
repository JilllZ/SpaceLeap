using UnityEngine;
using Leap;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent (typeof(GrabbingInfoSettings))]
public class InteractionController : MonoBehaviour {
    [Tooltip("Which method to use to push objects")]
    public bool enablePushing = true;
    [Tooltip("Whether or not to allow objects to be grabbed")]
    public bool enableGrabbing = true;

    [Tooltip("All objects on this layer will colliders will be grabbed")]
    public LayerMask grabbableLayer;

    [MinValue(0)]
    [Tooltip("How long after releasing an object can it be pushed again")]
    public float releasePushCooldown = 0.75f;

    [MinValue(0)]
    [Tooltip("How long after a hand has passed out of view to wait before releasing an object")]
    public float handDisapearDelay = 5.0f;

	public GrabbingInfo grabSettings;

    //Maps objects to the time they should be re-enabled
    private Dictionary<InteractionObject, float> _objectsToReEnable = new Dictionary<InteractionObject, float>();

    //Set of all hands that are currently being tracked by the system
    private HashSet<InteractionHand> _trackingHands = new HashSet<InteractionHand>();

    private Dictionary<InteractionObject, KabschMovementSolver> _solvers = new Dictionary<InteractionObject, KabschMovementSolver>();

    private GrabbingInfoSettings _grabbingInfoSettings;

    public static Action<GameObject> onObjectGrabbed;
    public static Action<GameObject> onObjectReleased;

    #region PUBLIC_GETTERS
    private static InteractionController _instance = null;

    public static InteractionController instance {
        get {
            return _instance;
        }
    }

    public static GrabbingInfoSettings grabbingInfoSettings {
        get {
            return instance._grabbingInfoSettings;
        }
    }

    public static bool isHandGrabbing(int handId) {
        foreach (InteractionHand kHand in instance._trackingHands) {
            if (kHand.leapHand.Id == handId && kHand.isGrabbing) {
                return true;
            }
        }
        return false;
    }

    public static bool isHandGrabbing(Hand hand) {
        foreach (InteractionHand kHand in instance._trackingHands) {
            if (kHand.leapHand.Id == hand.Id && kHand.isGrabbing) {
                return true;
            }
        }
        return false;
    }

    public static bool isAnyHandGrabbing() {
        foreach (InteractionHand kHand in instance._trackingHands) {
            if (kHand.isGrabbing) {
                return true;
            }
        }
        return false;
    }

    public static bool isAnyHandPushing() {
        foreach (InteractionHand kHand in instance._trackingHands) {
            if (kHand.isTouching) {
                return true;
            }
        }
        return false;
    }

    public static GameObject getGrabbedObject(Hand hand) {
        foreach (InteractionHand kHand in instance._trackingHands) {
            if (kHand.leapHand.Id == hand.Id && kHand.grabbedObject != null) {
                return kHand.grabbedObject.gameObject;
            }
        }
        return null;
    }

    #endregion

    private Controller _leapController;

    private Frame mostRecentFrame;

    void Awake() {
        _instance = this;
        _grabbingInfoSettings = GetComponent<GrabbingInfoSettings>();
    }

    void OnDisable() {
        _objectsToReEnable.Clear();
        _trackingHands.Clear();
    }

    void OnDestroy() {
        _instance = null;
    }

    void Start() {
        _leapController = HandControllerUtil.handController.GetLeapController();
    }

    void FixedUpdate() {
        if (mostRecentFrame == null || mostRecentFrame.Id != _leapController.Frame().Id) {
            mostRecentFrame = _leapController.Frame();

            InteractionObject.syncAllInteractionObjects();

            updateHands();

            if (enablePushing) {
                handlePushingDisconnection();
            }

            if (enableGrabbing) {
                handleGrabbingConnection();
            }

            solveAllObjects();

            if (enablePushing) {
                handlePushingConnection();
            }

            if (enableGrabbing) {
                handleGrabbingDisconnection();
            }

            updateRigidbodyStatus();
        }
    }

    private void updateRigidbodyStatus() {
        HandModel[] physicsModels = HandControllerUtil.handController.GetAllPhysicsHands();

        HashSet<InteractionObject> grabbedObjects = new HashSet<InteractionObject>();

        //Maps objects to the time they should be re-enabled
        Dictionary<InteractionObject, float> relevantObjects = new Dictionary<InteractionObject, float>();

        foreach (InteractionHand kHand in _trackingHands) {
            if (kHand.grabbedObject == null) {
                continue;
            }
            if (kHand.grabbedObject.rigidbody == null) {
                continue;
            }

            float t;
            if (relevantObjects.TryGetValue(kHand.grabbedObject, out t)) {
                t = Mathf.Max(t, kHand.recentReleaseTime);
            } else {
                t = kHand.recentReleaseTime;
            }
            relevantObjects[kHand.grabbedObject] = t + releasePushCooldown;

            if (kHand.isGrabbing) {
                grabbedObjects.Add(kHand.grabbedObject);
            }
        }

        foreach (InteractionObject interactionObject in relevantObjects.Keys) {
            bool isGrabbed = grabbedObjects.Contains(interactionObject);

            if (isGrabbed && !interactionObject.rigidbody.isKinematic) {
                interactionObject.rigidbody.velocity = Vector3.zero;
                interactionObject.rigidbody.angularVelocity = Vector3.zero;
                interactionObject.rigidbody.isKinematic = true;
                interactionObject.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                foreach (HandModel model in physicsModels) {
                    Leap.Utils.IgnoreCollisions(interactionObject.gameObject, model.gameObject, true);
                }
            }
            if (!isGrabbed && interactionObject.rigidbody.isKinematic) {
                interactionObject.rigidbody.isKinematic = false;

                float speed = interactionObject.rigidbody.velocity.magnitude;
                Vector3 velNorm = interactionObject.rigidbody.velocity / speed;
                velNorm.y *= 2.5f;
                interactionObject.rigidbody.velocity = velNorm.normalized * speed;

                _objectsToReEnable[interactionObject] = relevantObjects[interactionObject];
            }
        }

        HashSet<InteractionObject> keysToDelete = new HashSet<InteractionObject>();
        foreach (var pair in _objectsToReEnable) {
            if (Time.time >= pair.Value) {
                foreach (HandModel model in physicsModels) {
                    Leap.Utils.IgnoreCollisions(pair.Key.gameObject, model.gameObject, false);
                }
                keysToDelete.Add(pair.Key);
            }
        }

        foreach (InteractionObject obj in keysToDelete) {
            _objectsToReEnable.Remove(obj);
        }
    }

    private void updateHands() {
        foreach (Hand hand in mostRecentFrame.Hands) {
            InteractionHand matchingHand = _trackingHands.FirstOrDefault(h => h.leapHand.Id == hand.Id);

            if (matchingHand == null) {
                //If the new hand does not have the same ID as any hand we have been tracking
                //See if it matches the handedness of a hand that has been untracked for a little bit of time
                InteractionHand matchingStaleHand = _trackingHands.FirstOrDefault(h => h.timeSinceUpdated > 0.1f && h.leapHand.IsRight == hand.IsRight);
                if (matchingStaleHand != null) {
                    //if it does, the new hand assumes control of the abandonded hand
                    matchingStaleHand.updateInfo(hand);
                } else {
                    //otherwise we create a new hand to be tracked 
                    InteractionHand newHand = new InteractionHand();
                    newHand.updateInfo(hand);
                    _trackingHands.Add(newHand);
                }
            } else {
                //We found the match, simply update it
                matchingHand.updateInfo(hand);
            }
        }

        HandModel[] physicsModels = HandControllerUtil.handController.GetAllPhysicsHands();

        //All hands that haven't been updated for too long
        foreach (InteractionHand kHand in _trackingHands.Where(h =>
            h.timeSinceUpdated > handDisapearDelay &&
            h.grabbedObject != null &&
            h.cachedRigidbody != null &&
            h.cachedRigidbody.isKinematic)) {
            kHand.cachedRigidbody.isKinematic = false;
            foreach (HandModel model in physicsModels) {
                Leap.Utils.IgnoreCollisions(kHand.grabbedObject.gameObject, model.gameObject, false);
            }
        }

        //Remove all the stale hands from the set
        _trackingHands.RemoveWhere(h => h.timeSinceUpdated > handDisapearDelay);
    }

    private void handlePushingConnection() {
        foreach (InteractionHand kHand in _trackingHands.Where(
            h => !h.isGrabbing &&
            h.timeSinceRelease > releasePushCooldown)) {
            kHand.connectFingerPointsIfInside(InteractionPointConnectMethod.SURFACE);
        }
    }

    private void handlePushingDisconnection() {
        foreach (InteractionHand kHand in _trackingHands.Where(h => !h.isGrabbing)) {
            kHand.disconnectFingerPointsIfOutside();
        }
    }

    private void handleGrabbingConnection() {
        foreach (InteractionHand kHand in _trackingHands.Where(h => !h.isGrabbing)) {
            if (kHand.tryGrabObject()) {
                if(onObjectGrabbed != null) onObjectGrabbed(kHand.grabbedObject.gameObject);
                if (kHand.grabbedObject.onGrab != null) kHand.grabbedObject.onGrab(kHand.leapHand);
            }
        }
    }

    private void handleGrabbingDisconnection() {
        foreach (InteractionHand kHand in _trackingHands.Where(h => h.isGrabbing)) {
            if (kHand.tryReleaseObject()) {
                if(onObjectReleased != null) onObjectReleased(kHand.grabbedObject.gameObject);
                if (kHand.grabbedObject.onRelease != null) kHand.grabbedObject.onRelease(kHand.leapHand);
            }
        }
    }

    private void solveAllObjects() {
        foreach (InteractionHand kHand in _trackingHands) {
            foreach (InteractionPoint kPoint in kHand.connectedPoints) {
                KabschMovementSolver solver;
                if (!_solvers.TryGetValue(kPoint.interactionObject, out solver)) {
                    solver = new KabschMovementSolver(kPoint.interactionObject);
                    _solvers[kPoint.interactionObject] = solver;
                }
                solver.addPoint(kPoint);

                if (kHand.isGrabbing) {
                    solver.useGrabbedMethod();
                }
            }
        }

        foreach (KabschMovementSolver solver in _solvers.Values) {
            solver.solve();
        }
    }

    public void OnDrawGizmos() {
        HashSet<GameObject> allCloseObjects = new HashSet<GameObject>();

        Gizmos.color = Color.red;
        foreach (InteractionHand kHand in _trackingHands) {
            allCloseObjects.UnionWith(kHand.closeObjects);

            foreach (InteractionPoint kPoint in kHand.trackedPoints) {
                Gizmos.DrawSphere(kPoint.globalPosition, 0.005f);

                if (kPoint.isConnected) {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(kPoint.globalReferencePosition, 0.005f);
                    GizmoUtil.DrawLine(kPoint.globalPosition, kPoint.globalReferencePosition);
                }
            }
        }

        GizmoUtil.pushMatrix();
        foreach (GameObject obj in allCloseObjects.Where(o => o != null)) {
            GizmoUtil.relativeTo(obj);
            InteractionObject interactionObject = InteractionObject.getInteractionObject(obj);

            if (interactionObject.rigidbody == null) {
                Gizmos.color = Color.blue;
            } else {
                Gizmos.color = obj.GetComponent<Rigidbody>().isKinematic ? Color.green : Color.magenta;
            }

            if (_objectsToReEnable.ContainsKey(interactionObject)) {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 1.1f);
        }
        GizmoUtil.popMatrix();
    }
}
