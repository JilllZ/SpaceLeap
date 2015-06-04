using UnityEngine;
using Leap;
using System.Collections;
using System.Collections.Generic;

public class InteractionObject {
    #region Shape Classes
    public interface IShape {
        //Point is local to the poition and transform of this collider representation
        float distanceTo(Vector3 localPoint, float extrude);
        Vector3 closestPointOnSurface(Vector3 localPoint, float extrude);
    }

    public struct Capsule : IShape { 
        public readonly Vector3 v0;
        public readonly Vector3 v1;
        public readonly float radius;
        public readonly float length;

        public Capsule(CapsuleCollider c, GameObject anchor) {
            Vector3 axis = Vector3.right;
            if (c.direction == 1) {
                axis = Vector3.up;
            } else {
                axis = Vector3.forward;
            }

            length = Mathf.Max(0, c.height - c.radius * 2);
            Vector3 localV0 = axis * length / 2.0f + c.center;
            Vector3 localV1 = -axis * length / 2.0f + c.center;

            v0 = anchor.transform.InverseTransformPoint(c.transform.TransformPoint(localV0));
            v1 = anchor.transform.InverseTransformPoint(c.transform.TransformPoint(localV1));
            radius = anchor.transform.InverseTransformVector(c.transform.TransformVector(Vector3.right * c.radius)).x;
        }

        public float distanceTo(Vector3 localPoint, float extrude) {
            if (length == 0.0f) {
                return Vector3.Distance(localPoint, v0) - (radius + extrude);
            }

            float t = Mathf.Clamp01(Vector3.Dot(localPoint - v0, v1 - v0) / (length * length));
            Vector3 projection = v0 + t * (v1 - v0);
            return Vector3.Distance(localPoint, projection) - (radius + extrude);
        }

        public Vector3 closestPointOnSurface(Vector3 localPoint, float extrude) {
            if (length == 0.0f) {
                return v0 + (localPoint - v0).normalized * (radius + extrude);
            }

            float t = Mathf.Clamp01(Vector3.Dot(localPoint - v0, v1 - v0) / (length * length));
            Vector3 projection = v0 + t * (v1 - v0);
            return projection + (localPoint - projection).normalized * (radius + extrude);
        }
    }

    public struct Sphere : IShape{
        public readonly Vector3 center;
        public readonly float radius;

        public Sphere(SphereCollider c, GameObject anchor) {
            center = anchor.transform.InverseTransformPoint(c.transform.TransformPoint(c.center));
            radius = anchor.transform.InverseTransformVector(c.transform.TransformVector(Vector3.right * c.radius)).x;
        }

        public float distanceTo(Vector3 localPoint, float extrude) {
            return Vector3.Distance(localPoint, center) - (radius + extrude);
        }

        public Vector3 closestPointOnSurface(Vector3 localPoint, float extrude) {
            return center + (localPoint - center).normalized * (radius + extrude);
        }
    }

    public struct Box : IShape{
        public readonly Vector3 center;
        public readonly Vector3 radius;
        public readonly Quaternion rotation;

        public Box(BoxCollider c, GameObject anchor) {
            center = anchor.transform.InverseTransformPoint(anchor.transform.TransformPoint(c.center));
            radius = anchor.transform.InverseTransformVector(anchor.transform.TransformVector(c.size)) / 2.0f;
            rotation = Quaternion.Inverse(anchor.transform.rotation) * c.transform.rotation;
        }

        public float distanceTo(Vector3 localPoint, float extrude) {
            Vector3 relative = (localPoint - center);
            Vector3 offset = relative.abs() - (radius + Vector3.one * extrude);
            return Mathf.Min(offset.maxc(), 0) + Vector3.Max(offset, Vector3.zero).magnitude;
        }

        public Vector3 closestPointOnSurface(Vector3 localPoint, float extrude) {
            throw new System.NotImplementedException();
        }
    }
    #endregion

    public System.Action<Hand> onGrab;
    public System.Action<Hand> onRelease;

    private static Dictionary<GameObject, InteractionObject> _instanceMap = new Dictionary<GameObject, InteractionObject>();

    private List<Collider> _colliders = new List<Collider>();
    private List<IShape> _shapes = new List<IShape>();

    private Vector3 _position;
    private Quaternion _rotation;
    private float _scale;

    private bool _isCachedInverseRotationStale = true;
    private Quaternion _cachedInverseRotation;

    private GameObject _gameObject;
    private Rigidbody _rigidbody;

    public static InteractionObject getInteractionObject(GameObject obj) {
        InteractionObject interactionObject;
        if (!_instanceMap.TryGetValue(obj, out interactionObject)) {
            interactionObject = new InteractionObject(obj);
            _instanceMap[obj] = interactionObject;
        }
        return interactionObject;
    }

    public static void syncAllInteractionObjects() {
        foreach(InteractionObject obj in _instanceMap.Values){
            obj.position = obj.gameObject.transform.position;
            obj.rotation = obj.gameObject.transform.rotation;
        }
    }

    public GameObject gameObject { get { return _gameObject; } }

    public Rigidbody rigidbody { get { return _rigidbody; } }

    public Vector3 position {
        get { return _position; }
        set { _position = value; }
    }

    public Quaternion rotation {
        get { return _rotation; }
        set {
            _rotation = value;
            _isCachedInverseRotationStale = true;
        }
    }

    public InteractionObject(GameObject obj) {
        foreach (Collider c in obj.GetComponentsInChildren<Collider>()) {
            if (c is SphereCollider) {
                _shapes.Add(new Sphere(c as SphereCollider, obj));
                _colliders.Add(c);
            } else if (c is CapsuleCollider) {
                _shapes.Add(new Capsule(c as CapsuleCollider, obj));
                _colliders.Add(c);
            } else if (c is BoxCollider) {
                _shapes.Add(new Box(c as BoxCollider, obj));
                _colliders.Add(c);
            }
        }

        _gameObject = obj;
        _rigidbody = _gameObject.GetComponent<Rigidbody>();
        _scale = _gameObject.transform.localScale.x;
        _position = _gameObject.transform.position;
        _rotation = _gameObject.transform.rotation;
    }

    private Quaternion getInverseRotation() {
        if (_isCachedInverseRotationStale) {
            _cachedInverseRotation = Quaternion.Inverse(_rotation);
            _isCachedInverseRotationStale = false;
        }
        return _cachedInverseRotation;
    }

    public Vector3 getLocalPosition(Vector3 worldPosition) {
        return getInverseRotation() * ((worldPosition - _position) / _scale);
    }

    public Vector3 getWorldPosition(Vector3 localPosition) {
        return _rotation * (localPosition * _scale) + _position;
    }

    public Vector3 getLocalDirection(Vector3 worldDirection) {
        return getInverseRotation() * worldDirection;
    }

    public Vector3 getWorldDirection(Vector3 localDirection) {
        return _rotation * localDirection;
    }

    public float getLocalLength(float worldLength) {
        return worldLength / _scale;
    }

    public float getWorldLength(float localLength) {
        return localLength * _scale;
    }

    public float signedDistanceToL2L(Vector3 localPosition, float worldExtrude = 0.0f) {
        float minDist = _shapes[0].distanceTo(localPosition, getWorldLength(worldExtrude));
        for (int i = 1; i < _shapes.Count; i++) {
            float dist = _shapes[i].distanceTo(localPosition, getWorldLength(worldExtrude));
            if (Mathf.Abs(dist) < Mathf.Abs(minDist)) {
                minDist = dist;
            }
        }

        return minDist;
    }

    public float signedDistanceToL2W(Vector3 localPosition, float worldExtrude = 0.0f) {
        return getWorldLength(signedDistanceToL2L(localPosition, worldExtrude));
    }

    public float signedDistanceToW2L(Vector3 worldPosition, float worldExtrude = 0.0f) {
        return signedDistanceToL2L(getLocalPosition(worldPosition), worldExtrude);
    }

    public float signedDistanceToW2W(Vector3 worldPosition, float worldExtrude = 0.0f) {
        return getWorldLength(signedDistanceToW2L(worldPosition, worldExtrude));
    }

    public Vector3 closestPointOnSurfaceL2L(Vector3 localPosition, float worldExtrude = 0.0f) {
        float minDist = _shapes[0].distanceTo(localPosition, getLocalLength(worldExtrude));
        IShape closestShape = _shapes[0];

        for (int i = 1; i < _shapes.Count; i++) {
            float dist = _shapes[i].distanceTo(localPosition, getLocalLength(worldExtrude));
            if (Mathf.Abs(dist) < Mathf.Abs(minDist)) {
                minDist = dist;
                closestShape = _shapes[i];
            }
        }

        return closestShape.closestPointOnSurface(localPosition, getLocalLength(worldExtrude));
    }

    public Vector3 closestPointOnSurfaceL2W(Vector3 localPosition, float worldExtrude = 0.0f) {
        return getWorldPosition(closestPointOnSurfaceL2L(localPosition, worldExtrude));
    }

    public Vector3 closestPointOnSurfaceW2L(Vector3 worldPosition, float worldExtrude = 0.0f) {
        return closestPointOnSurfaceL2L(getLocalPosition(worldPosition), worldExtrude);
    }

    public Vector3 closestPointOnSurfaceW2W(Vector3 worldPosition, float worldExtrude = 0.0f) {
        return getWorldPosition(closestPointOnSurfaceW2L(worldPosition, worldExtrude));
    }

    public bool segmentCastW(Vector3 worldStart, Vector3 worldEnd, out RaycastHit hitInfo) {
        Ray ray = new Ray(worldStart, worldEnd - worldStart);
        float maxDist = Vector3.Distance(worldStart, worldEnd);
        float minDist = float.MaxValue;

        hitInfo = new RaycastHit();

        foreach (Collider c in _colliders) {
            RaycastHit tempHit = new RaycastHit();
            if (c.Raycast(ray, out tempHit, maxDist)) {
                if (tempHit.distance < minDist) {
                    minDist = tempHit.distance;
                    hitInfo = tempHit;
                }
            }
        }

        return minDist != float.MaxValue;
    }

    public bool isInsideL(Vector3 localPosition, float worldExtrude = 0.0f) {
        return signedDistanceToL2L(localPosition, getLocalLength(worldExtrude))  < 0.0f;
    }

    public bool isInsideW(Vector3 worldPosition, float worldExtrude = 0.0f) {
        return isInsideL(getLocalPosition(worldPosition), worldExtrude);
    }
}
