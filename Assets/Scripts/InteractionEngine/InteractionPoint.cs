using UnityEngine;
using System.Collections;


public enum InteractionPointConnectMethod {
    MIRROR,     //local point located at the same location as the global point
    SURFACE,    //local point is closest point on the surface of the object
    OUTSIDE,    //local point is closest point outside the object
    INSIDE      //local point is closest point inside the object
}

public class InteractionPoint {
    private Vector3 _referenceOffset;
	private Vector3 _referenceNormal;

    private InteractionObject _interactionObject;

    private Vector3 _globalPosition;

	private Vector3 _previousGlobalPosition;
    private bool _isConnected = false;

    public InteractionObject interactionObject {
        get {
            return _interactionObject;
        }
    }

    public Vector3 globalPosition {
        get {
            return _globalPosition;
        }
    }

    public Vector3 globalReferencePosition {
        get {
            return _interactionObject.getWorldPosition(_referenceOffset);
        }
    }

	public Vector3 globalReferenceNormal {
		get {
            return _interactionObject.getWorldDirection(_referenceNormal);
		}
	}

    public Vector3 localReferencePosition {
        get {
            return _referenceOffset;
        }
    }

	public Vector3 localReferenceNormal {
		get {
			return _referenceNormal;
		}
	}

    public bool isConnected {
        get {
            return _isConnected;
        }
    }

    public InteractionPoint() {
        _referenceOffset = Vector3.zero;
        _globalPosition = Vector3.zero;
		_previousGlobalPosition = Vector3.zero;
        _interactionObject = null;
        _isConnected = false;
    }

    public void update(Vector3 globalPosition) {
		_previousGlobalPosition = _globalPosition;
        _globalPosition = globalPosition;
    }

    public void connectToObject(InteractionObject obj, InteractionPointConnectMethod connectMode = InteractionPointConnectMethod.MIRROR) {
        _interactionObject = obj;

        if (connectMode == InteractionPointConnectMethod.MIRROR) {
            connectToObjectMirror(obj);
        } else {
            if (connectMode == InteractionPointConnectMethod.SURFACE) {
                connectToObjectSurface(obj);
            } else {
                if (obj.isInsideW(_globalPosition, InteractionHand.SIZE) == (connectMode == InteractionPointConnectMethod.INSIDE)) {
                    connectToObjectMirror(obj);
                } else {
                    connectToObjectSurface(obj);
                }
            }
        }

        _isConnected = true;
    }

    public bool tryConnectForPush(InteractionObject obj) {
        if (_isConnected) {
            Debug.LogError("Should not be trying to connect for push if already connected!");
        }

        if (!obj.isInsideW(_globalPosition)) {
            return false;
        }

		Vector3 localSurfacePoint;

		RaycastHit hitInfo;
        if (obj.segmentCastW(_previousGlobalPosition, _globalPosition, out hitInfo)) {
			localSurfacePoint = obj.getLocalPosition(hitInfo.point);
			//worldSurfaceNormal = hitInfo.normal;
		} else {
            localSurfacePoint = obj.closestPointOnSurfaceW2L(_globalPosition);
			//worldSurfaceNormal = worldSurfacePoint - globalPosition;
		}

        _interactionObject = obj;
        _referenceOffset = localSurfacePoint;
        //_referenceNormal = _interactionObject.getLocalDirection(worldSurfaceNormal);

        _isConnected = true;

        return true;
    }

    public bool tryDisconnectFromPush() {
        if (!_isConnected) {
            Debug.LogError("Should not be trying to disconnect from push if not connected!");
        }

        if (!_interactionObject.isInsideW(_globalPosition)) {
            _isConnected = false;
            return true;
        }

        return false;
    }

    public void forceMirror() {
        _referenceOffset = _interactionObject.getLocalPosition(globalPosition);
    }

    public void disconnect() {
        _isConnected = false;
    }

    private void connectToObjectMirror(InteractionObject obj) {
        _referenceOffset = obj.getLocalPosition(_globalPosition);
    }

    private void connectToObjectSurface(InteractionObject obj) {
        _referenceOffset = obj.closestPointOnSurfaceW2L(_globalPosition, InteractionHand.SIZE);
    }
}
