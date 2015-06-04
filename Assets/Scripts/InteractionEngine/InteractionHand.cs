using UnityEngine;
using Leap;
using System.Collections;
using System.Collections.Generic;

public class InteractionHand {
    public const float PALM_BASIS_WEIGHT = 0.1f;
    public const float SIZE = 0.01f;

    private Dictionary<Finger.FingerType, Dictionary<Finger.FingerJoint, InteractionPoint>> _fingerPointMap = new Dictionary<Finger.FingerType, Dictionary<Finger.FingerJoint, InteractionPoint>>();
    private InteractionPoint[] _basisPoints = new InteractionPoint[6];

    private Hand _leapHand;

    private float _lastTimeUpdated = 0.0f;
    private bool _isGrabbingObject = false;
    private InteractionObject _grabbedObject = null;
    private float _lastTimeReleased = 0.0f;

    private Rigidbody _cachedRigidbody = null;

    private HashSet<GameObject> _closeObjects = new HashSet<GameObject>();

    public Rigidbody cachedRigidbody {
        get {
            return _cachedRigidbody;
        }
    }

    public HashSet<GameObject> closeObjects {
        get {
            return _closeObjects;
        }
    }

    public InteractionHand() {
        for (int i = 0; i < 6; i++) {
            _basisPoints[i] = new InteractionPoint();
        }
    }

    #region PUBLIC_GETTERS

    public Hand leapHand { get { return _leapHand; } }

    public bool isGrabbing { get { return _isGrabbingObject; } }

    public bool isTouching{
        get{ 
            if(_isGrabbingObject){
                return true;
            }
            foreach (var p in _basisPoints) {
                if (p.isConnected) {
                    return true;
                }
            }
            return false;
        }
    }

    public InteractionObject grabbedObject { get { return _grabbedObject; } }

    public float timeSinceUpdated { get { return Time.time - _lastTimeUpdated; } }

    public float timeSinceRelease { get { return Time.time - _lastTimeReleased; } }

    public float recentReleaseTime { get { return _lastTimeReleased; } }

    public IEnumerable<InteractionPoint> trackedFingerPoints {
        get {
            foreach (var d in _fingerPointMap.Values) {
                foreach (var p in d.Values) {
                    yield return p;
                }
            }
        }
    }

    public IEnumerable<InteractionPoint> connectedFingerPoints {
        get {
            foreach (var d in _fingerPointMap.Values) {
                foreach (var p in d.Values) {
                    if (p.isConnected) {
                        yield return p;
                    }
                }
            }
        }
    }

    public IEnumerable<InteractionPoint> trackedPoints {
        get {
            foreach (var d in _fingerPointMap.Values) {
                foreach (var p in d.Values) {
                    yield return p;
                }
            }
            foreach (InteractionPoint p in _basisPoints) {
                yield return p;
            }
        }
    }

    public IEnumerable<InteractionPoint> connectedPoints {
        get {
            foreach (var d in _fingerPointMap.Values) {
                foreach (var p in d.Values) {
                    if (p.isConnected) {
                        yield return p;
                    }
                }
            }
            foreach (InteractionPoint p in _basisPoints) {
                if (p.isConnected) {
                    yield return p;
                }
            }
        }
    }

    #endregion

    public void updateInfo(Hand hand) {
        _lastTimeUpdated = Time.time;
        _leapHand = hand;

        foreach (Finger finger in hand.Fingers) {

            Dictionary<Finger.FingerJoint, InteractionPoint> fingerPoints;
            if (!_fingerPointMap.TryGetValue(finger.Type, out fingerPoints)) {
                fingerPoints = new Dictionary<Finger.FingerJoint, InteractionPoint>();
                _fingerPointMap[finger.Type] = fingerPoints;
            }

            for (int i = 0; i < 4; i++) {
                Finger.FingerJoint jointType = (Finger.FingerJoint)i;

                Vector3 jointPosition = HandInfo.unityPos(finger.JointPosition(jointType));

                InteractionPoint point;
                if (!fingerPoints.TryGetValue(jointType, out point)) {
                    point = new InteractionPoint();
                    fingerPoints[jointType] = point;
                }

                point.update(jointPosition);
            }
        }

        Vector3 palmPoint = HandInfo.unityPos(hand.PalmPosition);

        Matrix basis = hand.Basis;
        Vector3 xBasis = HandInfo.unityDir(basis.xBasis) * PALM_BASIS_WEIGHT;
        Vector3 yBasis = HandInfo.unityDir(basis.yBasis) * PALM_BASIS_WEIGHT;
        Vector3 zBasis = HandInfo.unityDir(basis.zBasis) * PALM_BASIS_WEIGHT;

        _basisPoints[0].update(palmPoint + xBasis);
        _basisPoints[1].update(palmPoint + yBasis);
        _basisPoints[2].update(palmPoint + zBasis);
        _basisPoints[3].update(palmPoint - xBasis);
        _basisPoints[4].update(palmPoint - yBasis);
        _basisPoints[5].update(palmPoint - zBasis);

        _closeObjects.Clear();
        Collider[] colliders = Physics.OverlapSphere(palmPoint, HandInfo.unityLength(0.25f), InteractionController.instance.grabbableLayer);
        foreach (Collider c in colliders) {
            _closeObjects.Add(c.gameObject);
        }
    }

    public void connectBasisPoints(InteractionObject obj) {
        foreach (InteractionPoint p in _basisPoints) {
            p.connectToObject(obj, InteractionPointConnectMethod.MIRROR);
        }
    }

    public void connectAllFingerPoints(InteractionObject obj, InteractionPointConnectMethod method) {
        foreach (InteractionPoint p in trackedFingerPoints) {
            p.connectToObject(obj, method);
        }
    }

    public void disconnectAllFingerPoints() {
        foreach (InteractionPoint p in trackedFingerPoints) {
            p.disconnect();
        }
    }

    public void disconnectAllBasisPoints() {
        foreach (InteractionPoint p in _basisPoints) {
            p.disconnect();
        }
    }

    public void connectFingerPointsIfInside(InteractionPointConnectMethod method) {
        foreach (GameObject closeObject in _closeObjects) {
            foreach (InteractionPoint p in trackedFingerPoints) {
                if (!p.isConnected) {
                    InteractionObject interactionObject = InteractionObject.getInteractionObject(closeObject);
                    p.tryConnectForPush(interactionObject);
                }
            }
        }
    }

    public void disconnectFingerPointsIfOutside() {
        foreach (InteractionPoint p in trackedFingerPoints) {
            if (p.isConnected) {
                p.tryDisconnectFromPush();
            }
        }
    }

    public void forceMirrorOfFingerPoints() {
        foreach (InteractionPoint p in trackedFingerPoints) {
            p.forceMirror();
        }
    }

    public void forceMirrorOfBasisPoints() {
        foreach (InteractionPoint p in _basisPoints) {
            p.forceMirror();
        }
    }

    public bool tryGrabObject() {
        foreach (GameObject closeObject in _closeObjects) {
            InteractionObject interactionObject = InteractionObject.getInteractionObject(closeObject);
            if (GrabbingInfo.shouldGrab(InteractionController.grabbingInfoSettings, _leapHand, interactionObject)) {
                _isGrabbingObject = true;
                _grabbedObject = interactionObject;
                _cachedRigidbody = _grabbedObject.rigidbody;
                connectAllFingerPoints(_grabbedObject, InteractionPointConnectMethod.SURFACE);
                connectBasisPoints(_grabbedObject);
                DebugGraph.Write("Grabbed");
                return true;
            }
        }
        return false;
    }

    public bool tryReleaseObject() {
        if (GrabbingInfo.shouldRelease(InteractionController.grabbingInfoSettings, _leapHand, _grabbedObject)) {
            _lastTimeReleased = Time.time;
            _isGrabbingObject = false;
            disconnectAllBasisPoints();
            disconnectAllFingerPoints();
			DebugGraph.Write("Released");
            return true;
        }
        return false;
    }
}
